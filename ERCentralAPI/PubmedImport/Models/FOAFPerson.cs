using System;

namespace PubmedImport
{
    public class FOAFPerson
    {
        //http://xmlns.com/foaf/spec/#term_givenName
        public string GivenName { get; set; }

        //http://xmlns.com/foaf/spec/#term_familyName
        public string FamilyName { get; set; }

        //http://xmlns.com/foaf/spec/#term_name
        public string Name { get; set; }

        public FOAFPerson()
        {
            GivenName = "";
            FamilyName = "";
            Name = "";
        }
    }
	public class Author : FOAFPerson
	{
        public int AuthorshipLevel { get; set; }
        public int Rank { get; set; }
        public Author() : base()
		{
			AuthorshipLevel = 0;//default, normal author; For book chapters, editors have AutorshipLevel = 1
            Rank = 0;
		}
	}
	 
}
