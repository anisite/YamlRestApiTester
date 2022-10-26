using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlRestApiTester.Commun
{
    public sealed class FrwNodeTypeResolver : INodeTypeResolver
    {
        bool INodeTypeResolver.Resolve(NodeEvent? nodeEvent, ref Type currentType)
        {
            if (currentType == typeof(object))
            {
                if (nodeEvent is SequenceStart)
                {
                    currentType = typeof(List<object>);
                    return true;
                }
                if (nodeEvent is MappingStart)
                {
                    currentType = typeof(Dictionary<string, object>);
                    return true;
                }
            }

            return false;
        }
    }

    public static class OutilsYaml
    {
        public static IDeserializer deserializer = new DeserializerBuilder()
             .WithNamingConvention(CamelCaseNamingConvention.Instance)
             .IgnoreUnmatchedProperties()
             .Build();

        public static IDeserializer deserializerAlias = new DeserializerBuilder()
         .WithNamingConvention(CamelCaseNamingConvention.Instance)
          // Workaround to remove YamlAttributesTypeInspector
          .WithTypeInspector(inner => inner, s => s.InsteadOf<YamlAttributesTypeInspector>())
          .WithTypeInspector(
              inner => new YamlAttributesTypeInspector(inner),
              s => s.Before<NamingConventionTypeInspector>()
          )
         .IgnoreUnmatchedProperties()
         .Build();

        public static IDeserializer deserializerTextes = new DeserializerBuilder()
             .WithNamingConvention(CamelCaseNamingConvention.Instance)
             .WithNodeTypeResolver(new FrwNodeTypeResolver())
             .WithTagMapping(new TagName("tag:yaml.org,2002:Function"), typeof(JsFunction))
             .IgnoreUnmatchedProperties()
             .Build();

        public static ISerializer serializer = new SerializerBuilder()
                     .WithEventEmitter(next => new FlowEverythingEmitter(next))
                     .WithNamingConvention(CamelCaseNamingConvention.Instance)
                     .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                     .Build();

        public static T LireFicher<T>(string filename)
        {
            T cfg;
            using (var configFile = new StreamReader(filename))
            {
                cfg = deserializer.Deserialize<T>(configFile);
            }
            return cfg;
        }

        public static T DeserializerStringTextes<T>(string configFile)
        {
            return deserializerTextes.Deserialize<T>(configFile);
        }

        public static T DeserializerString<T>(string configFile)
        {
            return deserializer.Deserialize<T>(configFile);
        }

        public static T DeserializerStringSupportAlias<T>(string configFile)
        {
            return deserializerAlias.Deserialize<T>(configFile);
        }

        public static string SerialiserString<T>(T value)
        {
           return serializer.Serialize(graph: value!);
        }

        public static void EcrireFichier<T>(T value, string filename)
        {
            using (var configFile = new StreamWriter(filename))
            {
                serializer.Serialize(configFile, graph: value!);
            }
        }
    }
}
