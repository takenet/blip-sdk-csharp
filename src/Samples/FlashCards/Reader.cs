namespace bot_flash_cards_blip_sdk_csharp
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;

    public static class Reader
    {
       public static List<Person> Run()
       {
           using (StreamReader reader = new StreamReader("people.json"))
            {
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<Person>>(json);
            }
       }
    }
}