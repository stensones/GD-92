namespace Router.Domain.Messages;

public static class MessageFactory
{
	public static IMessage Create(MessageTypes messageType)
	{
		switch (messageType)
		{
			//case MessageTypes.Mobilise_command:
			//	break;
			//case MessageTypes.Mobilise_message:
			//	break;
			//case MessageTypes.Page_officer:
			//	break;
			//case MessageTypes.Area_page_message:
			//	break;
			//case MessageTypes.Resource_status_request:
			//	break;
			//case MessageTypes.Mobilise_message_OLD:
			//	break;
			//case MessageTypes.Activate_peripheral:
			//	break;
			//case MessageTypes.Deactivate_peripheral:
			//	break;
			//case MessageTypes.Peripheral_status_request:
			//	break;
			//case MessageTypes.Reset_request:
			//	break;
			//case MessageTypes.Resource_status:
			//	break;
			//case MessageTypes.Duty_staffing_update:
			//	break;
			//case MessageTypes.Log_update:
			//	break;
			//case MessageTypes.Stop:
			//	break;
			//case MessageTypes.Make_up:
			//	break;
			//case MessageTypes.Interrupt_request:
			//	break;
			//case MessageTypes.Text_message:
			//	break;
			//case MessageTypes.Peripheral_status:
			//	break;
			//case MessageTypes.Reset:
			//	break;
			//case MessageTypes.Incident_notification:
			//	break;
			//case MessageTypes.Alert_crew:
			//	break;
			//case MessageTypes.Alert_status:
			//	break;
			//case MessageTypes.Alert_eng:
			//	break;
			case MessageTypes.Ack:
				return new AckMessage();
				break;
			//case MessageTypes.Nak:
			//	break;
			//case MessageTypes.Set_parameter:
			//	break;
			//case MessageTypes.Parameter_request:
			//	break;
			//case MessageTypes.Parameter:
			//	break;
			//case MessageTypes.Param_req_multiple:
			//	break;
			//case MessageTypes.Test:
			//	break;
			//case MessageTypes.Printer_status:
			//	break;
			//case MessageTypes.MTA_status_change:
			//	break;
			//case MessageTypes.Route_status:
			//	break;
			//case MessageTypes.Supplier_message:
			//	break;
			//case MessageTypes.Brigade_message:
			//	break;
			//case MessageTypes.Data_base_query:
			//	break;
			//case MessageTypes.Formatted_text:
			//	break;
			//case MessageTypes.Proforma_definition_query:
			//	break;
			//case MessageTypes.Proforma_definition:
			//	break;
			default:
				throw new ArgumentOutOfRangeException(nameof(messageType));
		}
	}
}
