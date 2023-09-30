using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FramePFX.Logger;

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
            IEnumerable<TypeInfo> types;
            try {
                types = assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException e) {
                AppLogger.WriteLine($"Encountered exception while accessing defined types for assembly '{assembly}'");
                foreach (Exception ex in e.LoaderExceptions) {
                    string str;
                    if (ex is FileNotFoundException fileNotFound && !string.IsNullOrEmpty(str = fileNotFound.FusionLog)) {
                        AppLogger.PushHeader("An assembly file was not found");
                        AppLogger.WriteLine(ex.ToString());
                        AppLogger.WriteLine("Fusion logs: " + str);
                        AppLogger.PopHeader();
                    }
                    else if (ex is FileLoadException loadException && !string.IsNullOrEmpty(str = loadException.FusionLog)) {
                        AppLogger.PushHeader("An assembly file was not found");
                        AppLogger.WriteLine(ex.ToString());
                        AppLogger.WriteLine("Fusion logs: " + str);
                        AppLogger.PopHeader();
                    }
                    else {
                        AppLogger.WriteLine(ex.ToString());
                    }
                }

                return;
            }

            foreach (TypeInfo typeInfo in types) {
                this.ScanType(typeInfo);
            }
        }

        public void ScanType(TypeInfo type) {
            foreach (KeyValuePair<Type, List<(TypeInfo, Attribute)>> pair in this.AccumulationMap) {
                if (!Attribute.IsDefined(type, pair.Key)) {
                    continue;
                }

                Attribute[] attributes = Attribute.GetCustomAttributes(type, pair.Key);
                if (attributes.Length != 0) {
                    foreach (Attribute attribute in attributes) {
                        pair.Value.Add((type, attribute));
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