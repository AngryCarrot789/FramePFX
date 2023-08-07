namespace FramePFX.Core.Utils
{
    public class NumberAverager
    {
        private readonly double[] averages;

        public int NextIndex { get; private set; }

        public int Count => this.averages.Length;

        public NumberAverager(int count)
        {
            this.averages = new double[count];
        }

        public void PushValue(double number)
        {
            if (this.NextIndex >= this.averages.Length)
            {
                this.NextIndex = 0;
            }

            this.averages[this.NextIndex++] = number;
        }

        public double GetAverage()
        {
            double average = 0;
            foreach (double elem in this.averages)
            {
                average += elem;
            }

            return average / (double) this.averages.Length;
        }
    }
}