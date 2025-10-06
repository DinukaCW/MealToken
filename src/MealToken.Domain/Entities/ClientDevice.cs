using MealToken.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class ClientDevice
	{
		public int ClientDeviceId { get; set; }
		public int TenantId { get; set; }
		public string DeviceName { get; set; }
		public string IpAddress { get; set; }
		public int Port { get; set; }
		public int MachineNumber { get; set; }
		public string SerialNo { get; set; }
		public string PrinterName { get; set; }
		public bool IsActive { get; set; }
		public int ReceiptHeightPixels { get; set; }
		public int ReceiptWidthPixels { get; set; }
		public DeviceShift DeviceShift {  get; set; }

	}
	
}
