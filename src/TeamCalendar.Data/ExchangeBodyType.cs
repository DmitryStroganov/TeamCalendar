namespace TeamCalendar.Data
{
    /// <summary>
    ///     The ExchangeBodyType enumeration represents the body type of an exchange item.
    /// </summary>
    public enum ExchangeBodyType
    {
        /// <summary>
        ///     The item was retrieved but the criteria specified not to retrieve the body.
        /// </summary>
        NotRetrieved,

        /// <summary>
        ///     The body contains html.
        /// </summary>
        Html,

        /// <summary>
        ///     The body is plain text.
        /// </summary>
        Text
    }
}