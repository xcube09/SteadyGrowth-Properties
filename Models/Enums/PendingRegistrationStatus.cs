namespace SteadyGrowth.Web.Models.Enums
{
    /// <summary>
    /// Status of a pending registration
    /// </summary>
    public enum PendingRegistrationStatus
    {
        /// <summary>
        /// Awaiting payment confirmation and admin approval
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Payment confirmed, user account created
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Registration rejected by admin
        /// </summary>
        Rejected = 2
    }
}
