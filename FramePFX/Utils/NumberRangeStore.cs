using System;

namespace FramePFX.Utils {
    public class NumberRangeStore {
        private readonly Node first;

        // Used for quickly checking if a key has been used
        private long highest;

        public NumberRangeStore() {
            this.first = new Node();
            this.first.range = new Range(0);
        }

        public bool put(long key) {
            if (key < 1) {
                throw new Exception("Key cannot be below 1, it must be 1 or above");
            }

            if (key > this.highest) {
                this.highest = key;
            }

            Node node = this.first;
            Node prev = node;
            Range range = node.range;
            while (true) {
                if (range.isBetween(key)) {
                    return false;
                }
                else if (range.isAbove(key)) {
                    long keySub1 = key - 1;
                    long keyAdd1 = key + 1;
                    if (node.range.max == keySub1) {
                        node.range.max++;
                        if (node.next == null) {
                            return true;
                        }
                        else if (node.next.range.min == keyAdd1) {
                            node.range.max = node.next.range.max;
                            node.next.Remove();
                            return true;
                        }
                    }
                    else {
                        prev = node;
                        node = node.next;
                        if (node == null) {
                            Node newNode = new Node();
                            newNode.range = new Range(key);
                            newNode.AddAfter(prev);
                            return true;
                        }
                        else {
                            range = node.range;
                        }
                    }
                }
                else if (range.isBelow(key)) {
                    if (node.prev == prev) {
                        long keySub1 = key - 1;
                        long keyAdd1 = key + 1;
                        bool prevIncrement = false;
                        if (prev.range.max == keySub1) {
                            prev.range.max++;
                            prevIncrement = true;
                        }

                        if (node.range.min == keyAdd1) {
                            if (prevIncrement) {
                                prev.range.max = node.range.max;
                                node.Remove();
                            }

                            return true;
                        }
                        else if (prevIncrement) {
                            return true;
                        }
                        else {
                            Node newNode = new Node();
                            newNode.range = new Range(key);
                            newNode.InsertBetween(prev, node);
                            return true;
                        }
                    }
                    else {
                        prev = node;
                        node = node.prev;
                        if (node == null) {
                            throw new Exception("Huh...");
                        }

                        range = node.range;
                    }
                }
                else {
                    throw new Exception("What.....");
                }
            }
        }

        public bool hasKey(long key) {
            if (key < 1) {
                throw new Exception("Key cannot be below 1, it must be 1 or above");
            }

            if (key <= this.highest) {
                Node node = this.first;
                while (node != null) {
                    if (node.range.isBetween(key)) {
                        return true;
                    }
                    else if (node.range.isBelow(key)) {
                        return false;
                    }
                    else {
                        // it's above, so just continue and ignore it
                        node = node.next;
                    }
                }
            }

            return false;
        }


        // [1-4] [7-20] [45-50]
        // Removing 11, goes to
        // [1-4] [7-10] [12-20] [45-50]
        //
        // [1-4] [7-20] [45-50]
        // Removing 20, goes to
        // [1-4] [7-19] [45-50]
        public bool remove(long key) {
            if (key > this.highest) {
                return false;
            }

            Node node = this.first;
            Node prev = node;
            Range range = node.range;
            while (true) {
                if (range.isBetween(key)) {
                    if (key == range.max) {
                        node.range.max--;
                        return true;
                    }
                    else if (key == range.min) {
                        node.range.min++;
                        return true;
                    }
                    else {
                        node.range.max = (key - 1);
                        Node newNode = new Node();
                        newNode.range = new Range(key + 1, range.max);
                        newNode.InsertAfter(node);
                        return true;
                    }
                }
                else if (range.isAbove(key)) {
                    prev = node;
                    node = node.next;
                    if (node == null) {
                        return false;
                    }
                    else {
                        range = node.range;
                    }
                }
                else if (range.isBelow(key)) {
                    if (node.prev == prev) {
                        return false;
                    }

                    prev = node;
                    node = node.prev;
                    if (node == null) {
                        return false;
                    }

                    range = node.range;
                }
                else {
                    throw new Exception("What.....");
                }
            }
        }

        public void clear() {
            Node next = this.first;
            while (next != null) {
                Node node = next;
                next = next.next;
                node.Invalidate();
            }

            this.first.Invalidate();
            this.first.range = new Range(0);
        }

        // [] --- [] --- []
        // [] --- []
        // []
        //        []
        //               []
        // [] --- US --- [] --- []
        // [] --- [] --- US --- []
        private class Node {
            public Node prev;
            public Node next;
            public Range range;

            public Node() {
            }

            public void AddAfter(Node node) {
                node.next = this;
                this.prev = node;
            }

            public void AddBefore(Node node) {
                node.prev = this;
                this.next = node;
            }

            public void InsertAfter(Node node) {
                if (node.next != null) {
                    this.next = node.next;
                    node.next.prev = this;
                }

                node.next = this;
                this.prev = node;
            }

            public void Remove() {
                // avoid multiple getfield opcode
                if (this.next == null || this.prev == null) {
                    if (this.prev != null) {
                        this.prev.next = null;
                        this.prev = null;
                    }
                    else if (this.next != null) {
                        this.next.prev = null;
                        this.next = null;
                    }
                }
                else {
                    this.prev.next = this.next;
                    this.next.prev = this.prev;
                    this.prev = null;
                    this.next = null;
                }
            }

            public void Invalidate() {
                this.next = null;
                this.prev = null;
            }

            public void InsertBetween(Node a, Node b) {
                if (a != null)
                    this.AddAfter(a);
                if (b != null)
                    this.AddBefore(b);
            }

            public override string ToString() {
                return $"[{this.range.min}-{this.range.max}]";
            }
        }

        private struct Range {
            public long min;
            public long max;

            public Range(long value) {
                this.min = value;
                this.max = value;
            }

            public Range(long min, long max) {
                this.min = min;
                this.max = max;
            }

            public bool isBetween(long value) => value >= this.min && value <= this.max;

            public bool isAbove(long value) => value > this.max;

            public bool isBelow(long value) => value < this.min;
        }
    }
}