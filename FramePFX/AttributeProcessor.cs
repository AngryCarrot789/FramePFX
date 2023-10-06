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

        private readonly Dictionary<Type, Action<TypeInfo, Attribute>> ProcessorMap;

        public AttributeProcessor() {
            this.AccumulationMap = new Dictionary<Type, List<(TypeInfo, Attribute)>>();
            this.ProcessorMap = new Dictionary<Type, Action<TypeInfo, Attribute>>();
        }

        public void RegisterProcessor<T>(Action<Type, T> processor) where T : Attribute {
            this.RegisterProcessorInternal(typeof(T), (t, a) => processor(t, (T) a));
        }

        private void RegisterProcessorInternal(Type attributeType, Action<TypeInfo, Attribute> processor) {
            if (!typeof(Attribute).IsAssignableFrom(attributeType))
                throw new ArgumentException($"Attribute type does not extend {nameof(Attribute)}", nameof(attributeType));
            if (processor == null)
                throw new ArgumentNullException(nameof(processor));
            if (this.AccumulationMap.ContainsKey(attributeType))
                throw new Exception("Attribute already registered:" + attributeType);

            this.AccumulationMap[attributeType] = new List<(TypeInfo, Attribute)>();
            this.ProcessorMap[attributeType] = processor;
        }

        /// <summary>
        /// Runs this attribute processor on the current <see cref="AppDomain"/>, which scans all assemblies and their types for their attributes
        /// </summary>
        public void ScanProcess() {
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
                        AppLogger.PushHeader("Failed to load an assembly file");
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

        /// <summary>
        /// Scans all attributes on the given type
        /// </summary>
        /// <param name="type"></param>
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

        /// <summary>
        /// Processes all attributes that were found (during the scan phase) of the given generic attribute
        /// </summary>
        /// <typeparam name="T">The type of attribute to process</typeparam>
        /// <exception cref="ArgumentNullException">Attribute type is null</exception>
        /// <exception cref="Exception">Attribute type was not registered with this processor</exception>
        public void Process<T>() where T : Attribute => this.Process(typeof(T));

        /// <summary>
        /// Processes all attributes that were found (during the scan phase) of the given type
        /// </summary>
        /// <param name="attributeType">The type of attribute to process</param>
        /// <exception cref="ArgumentNullException">Attribute type is null</exception>
        /// <exception cref="Exception">Attribute type was not registered with this processor</exception>
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
            foreach (KeyValuePair<Type,List<(TypeInfo, Attribute)>> pair in this.AccumulationMap)
                pair.Value.Clear();
            this.AccumulationMap.Clear();
            this.ProcessorMap.Clear();
        }
    }
}