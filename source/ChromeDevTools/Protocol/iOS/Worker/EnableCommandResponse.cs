using MasterDevs.ChromeDevTools;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MasterDevs.ChromeDevTools.Protocol.iOS.Worker
{
	[CommandResponse(ProtocolName.Worker.Enable)]
	[SupportedBy("iOS")]
	public class EnableCommandResponse
	{
	}
}
