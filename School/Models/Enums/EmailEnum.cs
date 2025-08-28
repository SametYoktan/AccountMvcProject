namespace School.Models.Enums
{
    public enum EmailDescriptionEnum
    {
        Hesap_Kilitlendi_Maili_Gönderildi=1,
        Şifre_Sıfırlama_Maili_Gönderildi=2,
        Hesap_Onaylama_Maili_Gönderildi=3
    }

    public enum EmailTypeEnum
    {
        Account = 1,
        PassWord = 2,
		Account_Confirmation=3
	}

    public enum IsUserActiveDescription
    {
        Hesap_Kilitlendi=1,
        Hesap_Aktifleştirildi=2
    }

	public enum IsAccountConfirmationDescription
	{
		Hesap_Onaylama = 1,
        Hesap_Onaylandı=2
    }
}
