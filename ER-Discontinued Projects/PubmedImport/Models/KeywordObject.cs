namespace PubmedImport
{
    public class KeywordObject
    {
		public string Name { get; set; }
		public bool Major { get; set; }

		public KeywordObject()
		{
			Name = "";
			Major = false;
		}

		public KeywordObject(string name)
			: this ()
		{
			Name = name;
		}
		public KeywordObject(string name, bool major)
		{
			Name = name;
			Major = major;
		}
	}
}
