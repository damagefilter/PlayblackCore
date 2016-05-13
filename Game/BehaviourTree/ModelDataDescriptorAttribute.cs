using System;
using System.Collections.Generic;
using Fasterflect;

namespace Playblack.BehaviourTree {

    /// <summary>
    /// Tack on modeltask classes to describe the data context of their corresponding executors
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModelDataDescriptorAttribute : System.Attribute {
        private string operatorName;
        private DescriptorType descriptorType;
        private Dictionary<string, FieldDefinitionAttribute> dataContextDescription;

        /// <summary>
        /// Constructor for full data description of stuffs.
        /// Yes stuffs.
        /// </summary>
        /// <param name="operatorName"></param>
        /// <param name="type"></param>
        /// <param name="dataContextDescription"></param>
        public ModelDataDescriptorAttribute(string operatorName, DescriptorType type, Type executorType) {
            this.operatorName = operatorName;
            this.descriptorType = type;
            // because we cannot pass nested attributes in c#, we'll just reflect the stuff to get hold of relevant context information.
            // Maybe it's for the better, I don't know ...
            var contextRelevantFields = executorType.FieldsWith(Flags.AnyVisibility, typeof(FieldDefinitionAttribute));
            var knownNames = new List<string>(contextRelevantFields.Count);

            this.dataContextDescription = new Dictionary<string, FieldDefinitionAttribute>(contextRelevantFields.Count);

            for (int i = 0; i < contextRelevantFields.Count; ++i) {
                var attr = contextRelevantFields[i].Attribute<FieldDefinitionAttribute>();
                if (knownNames.Contains(attr.DisplayName)) {
                    throw new ModelDataDescriptorException(string.Format(
                        "Execution Task for operator {0} has at least one double field label defined: {1} mapping to field {2}",
                        operatorName, attr.DisplayName, contextRelevantFields[i].Name));
                }
                dataContextDescription.Add(contextRelevantFields[i].Name, attr);
                knownNames.Add(attr.DisplayName);
            }
        }

        public Dictionary<string, FieldDefinitionAttribute> DataContextDescription {
            get {
                return dataContextDescription;
            }
        }

        public string OperatorName {
            get {
                return operatorName;
            }
            
        }

        public DescriptorType DescriptorType {
            get {
                return descriptorType;
            }
        }
    }
}
