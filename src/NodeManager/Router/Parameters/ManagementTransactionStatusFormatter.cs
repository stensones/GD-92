namespace NodeManager.Router.Parameters;

internal static class ManagementTransactionStatusFormatter
{
	public static string GetState(RouterParameterRequestStatus status)
	{
		ArgumentNullException.ThrowIfNull(status);

		return status switch
		{
			PendingRouterParameterRequestStatus pending => pending.State,
			PendingParameterModificationStatus pending => pending.State,
			ReceivedRouterParameterRequestStatus received => received.State,
			AcknowledgedParameterModificationStatus acknowledged => acknowledged.State,
			TimedOutRouterParameterRequestStatus timedOut => timedOut.State,
			DeliveryFailedRouterParameterRequestStatus deliveryFailed => deliveryFailed.State,
			RejectedRouterParameterRequestStatus rejected => rejected.State,
			PendingNodeLoginStatus pending => pending.State,
			PendingNodeLogoffStatus pending => pending.State,
			LoggedOnNodeLoginStatus loggedOn => loggedOn.State,
			LoggedOffNodeLoginStatus loggedOff => loggedOff.State,
			InvalidPasswordNodeLoginStatus invalidPassword => invalidPassword.State,
			RejectedNodeLoginStatus rejectedNodeLogin => rejectedNodeLogin.State,
			TimedOutNodeLoginStatus timedOutNodeLogin => timedOutNodeLogin.State,
			_ => throw new InvalidOperationException("The Management Transaction status is unknown.")
		};
	}
}
