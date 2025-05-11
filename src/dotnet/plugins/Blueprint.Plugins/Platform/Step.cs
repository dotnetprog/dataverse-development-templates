using System;

namespace Blueprint.Plugins.Platform
{
    public class Step
    {
        public readonly Stage Stage;
        public readonly Message Message;
        public readonly ExecutionMode mode;
        public readonly Action<ILocalPluginContext> Action;


        public string EntityName { get; set; }
        public Step(Stage pipelineStage, Message messageName, string entityName, ExecutionMode mode, Action<ILocalPluginContext> action)
        {
            this.Action = action;
            this.Stage = pipelineStage;
            this.Message = messageName;
            this.EntityName = entityName;
            this.mode = mode;
        }
        public Step(Stage pipelineStage, Message messageName, ExecutionMode mode, Action<ILocalPluginContext> action)
            : this(pipelineStage, messageName, string.Empty, mode, action)
        {

        }
    }
}
