using Fasterflect;
using System;
using System.Collections.Generic;

namespace Playblack.BehaviourTree {

    /// <summary>
    /// Tack on modeltask classes to describe the data context of their corresponding executors
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModelDataDescriptorAttribute : System.Attribute {
        private string operatorName;
        private DescriptorType descriptorType;
        private int numChildren;
        private Dictionary<string, FieldDefinitionAttribute> dataContextDescription;

        /// <summary>
        /// Hello
        /// </summary>
        /// <param name="operatorName">Display name of operator</param>
        /// <param name="type">The descriptor type, or operator type</param>
        /// <param name="acceptChildren">Does the described model use children? -1 says infinite, 0 = 0 etc etc</param>
        /// <param name="executorType">The ExecutionTask type on which to look for data fields</param>
        public ModelDataDescriptorAttribute(string operatorName, DescriptorType type, int numChildren, Type executorType) {
            this.operatorName = operatorName;
            this.descriptorType = type;
            this.numChildren = numChildren;
            // because we cannot pass nested attributes in c#, we'll just reflect the stuff to get hold of relevant context information.
            // Maybe it's for the better, I don't know ...
            var contextRelevantFields = executorType.FieldsWith(Flags.InstanceAnyVisibility, typeof(FieldDefinitionAttribute));
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

        public int NumChildren {
            get {
                return numChildren;
            }
        }
    }
}