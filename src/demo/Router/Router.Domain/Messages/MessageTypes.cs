namespace Router.Domain.Messages;

public enum MessageTypes
{
	Invalid = 0,

	Mobilise_command = 1,
	Mobilise_message = 2,
	Page_officer = 3,
	Area_page_message = 4,
	Resource_status_request = 5,
	Mobilise_message_OLD = 6,
	Activate_peripheral = 7,
	Deactivate_peripheral = 8,
	Peripheral_status_request = 9,
	Reset_request = 10,
	Resource_status = 20,
	Duty_staffing_update = 21,
	Log_update = 22,
	Stop = 23,
	Make_up = 24,
	Interrupt_request = 25,

	Text_message = 27,
	Peripheral_status = 28,
	Reset = 30,
	Incident_notification = 31,
	Alert_crew = 40,
	Alert_status = 42,
	Alert_eng = 43,

	Ack = 51,
	Nak = 52,

	Set_parameter = 60,
	Parameter_request = 61,
	Parameter = 62,
	Param_req_multiple = 63,
	Test = 64,
	Printer_status = 65,
	MTA_status_change = 66,
	Route_status = 67,

	Supplier_message = 100,
	Brigade_message = 101,
	Data_base_query = 102,
	Formatted_text = 103,
	Proforma_definition_query = 104,
	Proforma_definition = 105
}
