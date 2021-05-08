using MarcusW.VncClient.Protocol;
using MarcusW.VncClient.Protocol.EncodingTypes;
using MarcusW.VncClient.Protocol.Implementation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConnect.Protocols.Vnc
{
    public class VncProtocolImplementation : DefaultImplementation
    {
        public string PreferredEncodingType
        {
            get;
            set;
        }

        public override IImmutableSet<IEncodingType> CreateEncodingTypesCollection(RfbConnectionContext context)
        {
            if (String.IsNullOrEmpty(PreferredEncodingType))
            {
                return base.CreateEncodingTypesCollection(context);
            }

            List<IEncodingType> encodingTypes = GetDefaultEncodingTypes(context).ToList();
            IEncodingType preferredEncoding = encodingTypes.SingleOrDefault(t => t.Name == PreferredEncodingType);

            if (preferredEncoding != null)
            {
                encodingTypes.Remove(preferredEncoding);
                encodingTypes.Insert(0, preferredEncoding);
            }

            return encodingTypes.ToImmutableHashSet();
        }
    }
}
