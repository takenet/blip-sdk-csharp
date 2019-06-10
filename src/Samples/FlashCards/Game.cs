namespace bot_flash_cards_blip_sdk_csharp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata;
    using Lime.Messaging.Contents;
    using Lime.Protocol;

    public class Game
    {
        public string Player { get; set; }

        public int Questions { get; set; }

        public int Result { get; set; }

        public List<Person> People { get; set; }

        public List<Answer> Answers { get; set; }
        
        private Person _lastPerson;

        public MediaLink Run()
        {
            Random random = new Random();
            var person = random.Next(0, People.Count-1);

            var document = new MediaLink
            {
                Text = People[person].Text,
                Type = MediaType.Parse("image/jpeg"),
                AspectRatio = People[person].AspectRatio,
                Size = People[person].Size,
                Uri = new Uri(People[person].Uri, UriKind.Absolute),
                PreviewUri = new Uri(People[person].PreviewUri, UriKind.Absolute)
            };

            _lastPerson = People[person];
            People.RemoveAt(person);

            return document;
        }

        public void ProccessAnswer(string answerName)
        {
            var isCorrect = false;

            if (_lastPerson.Name.ToLower().Contains(answerName.ToLower()))
            {
                isCorrect = true;
            }

            Answers.Add(new Answer(isCorrect, _lastPerson, answerName));
        }

        public int ProccesResult() => Answers.Count((answer) => answer.IsCorrect);

        public IEnumerable<Answer> ProcessErrors()
        {
            var result = 
                from answer in Answers
                where !answer.IsCorrect
                select answer;

            return result;
        }

        public DocumentCollection ShowErros()
        {
            var documents = new DocumentSelect[ProcessErrors().Count()];
            var errors = ProcessErrors();
            var position = 0;

            foreach (var error in ProcessErrors())
            {
               documents[position] = 
                    new DocumentSelect
                    {
                        Header = new DocumentContainer
                        {
                            Value = new MediaLink
                            {
                                Title = error.Person.Name,
                                Text = $"You said {error.AnswerName}.",
                                Type = "image/jpeg",
                                Uri = new Uri(error.Person.Uri),
                            }
                        },
                        Options = new DocumentSelectOption[]
                        {
                            new DocumentSelectOption
                            {
                                Label = new DocumentContainer
                                {
                                    Value = new WebLink
                                    {
                                        Title = "Search on Workplace",
                                        Uri = new Uri("https://take.facebook.com/search/top/?q=" + error.Person.Name)
                                    }
                                }
                            }
                        }
                    };
                
                position++;
            }
            var document = new DocumentCollection
            {
                ItemType = "application/vnd.lime.document-select+json",
                Items = documents,
            };

            return document; 
        }
    }
}