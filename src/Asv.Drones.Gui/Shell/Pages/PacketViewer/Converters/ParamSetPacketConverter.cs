using System;
using System.Composition;
using System.Diagnostics;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;

namespace Asv.Drones.Gui;

[Export(typeof(IPacketConverter))]
public class ParamSetPacketConverter:IPacketConverter
{
    readonly IMavParamEncoding _cstyleEncoding = new MavParamCStyleEncoding();
    readonly IMavParamEncoding _byteWiseEncoding = new MavParamByteWiseEncoding();
    
    
    public bool CanConvert(IPacketV2<IPayload> packet)
    {
        return packet.MessageId == ParamSetPacket.PacketMessageId;
    }

    public string Convert(IPacketV2<IPayload> packet, PacketFormatting formatting = PacketFormatting.None)
    {
        var param = packet as ParamSetPacket;
        Debug.Assert(param != null, nameof(param) + " != null");
        
        var name = new Span<char>(param.Payload.ParamId).TrimEnd('\0').ToString();
        var cValue = _cstyleEncoding.ConvertFromMavlinkUnion(param.Payload.ParamValue, param.Payload.ParamType);
        var bValue = _byteWiseEncoding.ConvertFromMavlinkUnion(param.Payload.ParamValue, param.Payload.ParamType);
        return $"{name} = cstyle({cValue}) or byteWise({bValue})";
    }

    public int Order => int.MaxValue/2;
}