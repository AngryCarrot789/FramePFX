using System;
using System.Collections.Generic;
using System.Reflection;

namespace FramePFX {
    /// <summary>
    /// A class that makes processing attributes associated with every single type much easier
    /// </summary>
    public class AttributeProcessor {
        private readonly Dictionary<Type, List<(TypeInfo, Attribute)>> AccumulationMap;
        private readonly List<Type> OrderedAttributeTypeList;

        private readonly Dictionary<Type, Action<TypeInfo, Attribute>> ProcessorMap;

        public AttributeProcessor() {
            this.AccumulationMap = new Dictionary<Type, List<(TypeInfo, Attribute)>>();
            this.ProcessorMap = new Dictionary<Type, Action<TypeInfo, Attribute>>();
            this.OrderedAttributeTypeList = new List<Type>();
        }

        public void RegisterProcessor<T>(Action<Type, T> processor) where T : Attribute {
            this.RegisterProcessorInternal(typeof(T), (t, a) => processor(t, (T) a));
        }

        private void RegisterProcessorInternal(Type attributeType, Action<TypeInfo, Attribute> processor) {
            if (this.AccumulationMap.ContainsKey(attributeType)) {
                throw new Exception("Attribute already registered:" + attributeType);
            }

            this.OrderedAttributeTypeList.Add(attributeType);
            this.AccumulationMap[attributeType] = new List<(TypeInfo, Attribute)>();
            this.ProcessorMap[attributeType] = processor;
        }

        /// <summary>
        /// Runs this attribute processor, which scans all assemblies and their types for their attributes
        /// </summary>
        public void Run() {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                this.ScanAssembly(assembly);
            }
        }

        public void ScanAssembly(Assembly assembly) {
            foreach (TypeInfo typeInfo in assembly.DefinedTypes) {
                foreach (KeyValuePair<Type, List<(TypeInfo, Attribute)>> pair in this.AccumulationMap) {
                    if (!Attribute.IsDefined(typeInfo, pair.Key)) {
                        continue;
                    }

                    Attribute[] attributes = Attribute.GetCustomAttributes(typeInfo, pair.Key);
                    if (attributes.Length != 0) {
                        foreach (Attribute attribute in attributes) {
                            pair.Value.Add((typeInfo, attribute));
                        }
                    }
                }
            }
        }

        public void Process<T>() where T : Attribute => this.Process(typeof(T));

        public void Process(Type attributeType) {
            if (attributeType == null)
                throw new ArgumentNullException(nameof(attributeType));

            if (!this.AccumulationMap.TryGetValue(attributeType, out List<(TypeInfo, Attribute)> list)) {
                throw new Exception("Attribute type not registered: " + attributeType);
            }

            Action<TypeInfo, Attribute> processor = this.ProcessorMap[attributeType];
            foreach ((TypeInfo typeInfo, Attribute attribute) in list) {
                processor(typeInfo, attribute);
            }
        }

        public void Clear() {
            this.AccumulationMap.Clear();
            this.ProcessorMap.Clear();
        }
    }
}