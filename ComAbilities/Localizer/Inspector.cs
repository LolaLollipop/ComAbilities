using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization;

namespace ComAbilities.Localizer
{
    public class NewInfoInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector _innerTypeDescriptor;

        public NewInfoInspector(ITypeInspector innerTypeDescriptor)
        {
            _innerTypeDescriptor = innerTypeDescriptor;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            IEnumerable<IPropertyDescriptor> props = _innerTypeDescriptor.GetProperties(type, container);
            props = props.Where(p => !(p.Type == typeof(Dictionary<string, object>) && p.Name == "extensions"));
            props = props.Where(p => p.Name != "operation-id");
            return props;
        }
    }
}
