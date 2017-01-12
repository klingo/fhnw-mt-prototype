using System;

public class Transaction {

    public DateTime date { get; private set; }
    public String recipient { get; private set; }
    public String accountName { get; private set; }
    public String accountNo { get; private set; }
    public String currency { get; private set; }
    public float amount { get; private set; }
    public String bookingtext { get; private set; }
    public String category { get; private set; }

    // Constructor
    public Transaction(DateTime aDate, String aRecipient, String aAccountName, String aAccountNo, String aCurrency, float aAmount, String aBookingtext, String aCategory) {
        this.date = aDate;
        this.recipient = aRecipient;
        this.accountName = aAccountName;
        this.accountNo = aAccountNo;
        this.currency = aCurrency;
        this.amount = aAmount;
        this.bookingtext = aBookingtext;
        this.category = aCategory;
    }

}
