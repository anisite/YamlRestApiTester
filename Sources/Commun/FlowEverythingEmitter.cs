using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace YamlRestApiTester.Commun
{
    public class FlowEverythingEmitter : ChainedEventEmitter
    {
        public FlowEverythingEmitter(IEventEmitter nextEmitter) : base(nextEmitter) { }

        public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
        {
            if(eventInfo.Source.Value is List<string> list && list.Count == 1)
            {
                eventInfo.Style = SequenceStyle.Flow;
            }

            nextEmitter.Emit(eventInfo, emitter);
        }
    }
}