using IRS_PositionalIndexApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace IRS_PositionalIndexApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PositionalIndexController : Controller
    {
        private static List<Document> documents = new List<Document>
    {
        new Document { Id = 1, Text = "Call me when you get home" },
        new Document { Id = 2, Text = "He is forever complaining about this country" },
        new Document { Id = 3, Text = "If you cannot make it, call ME as soon as possible" },
        new Document { Id = 4, Text = "These are a few frequently asked questions about online courses." }
    };
        // Create a dictionary to store the positional index.
        static Dictionary<string, Dictionary<int, List<int>>> positionalIndex = new Dictionary<string, Dictionary<int, List<int>>>();

        private void CreatePositionalIndex()
        {
            // Read and process the content of the four text files.
            foreach (var doc in documents)
            {
                string fileContent = doc.Text;
                string[] words = fileContent.ToLower().Split(new char[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

                // Build the positional index for each word.
                int position = 1; // Initialize the position counter.
                foreach (string word in words)
                {
                    if (!positionalIndex.ContainsKey(word))
                    {
                        positionalIndex[word] = new Dictionary<int, List<int>>();
                    }

                    if (!positionalIndex[word].ContainsKey(doc.Id))
                    {
                        positionalIndex[word][doc.Id] = new List<int>();
                    }

                    positionalIndex[word][doc.Id].Add(position);

                    position++; // Increment the position counter.
                }
            }
        }
        [HttpPost("addDoc")]
        public ActionResult<IEnumerable<string>> AddDoc(string doc)
        {
            documents.Add(new Document { Id = (documents.Count() + 1), Text = doc });

            return Ok(documents);
        }
        [HttpGet("getMatrix")]
        public IActionResult GetPositionalIndexMatrix()
        {
            CreatePositionalIndex();


            return Ok(positionalIndex);
        }
        [HttpGet("getDoc")]
        public IActionResult GetDoc(int docId)
        {
            return Ok(documents.Where(doc => doc.Id == docId));
        }
        [HttpGet("searchDocIdtxt")]
        public IActionResult SearchDocIdtxt(string srchtxt)
        {
            if (string.IsNullOrEmpty(srchtxt))
                return BadRequest("Search text cannot be empty.");

            var srchtxtKey = srchtxt.ToLower().Split(new[] { ' ', '.', '!', '?', ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> res = new List<string>();
            if (srchtxtKey.Length == 1)
            {
                
                if (positionalIndex.ContainsKey(srchtxtKey[0]))
                {
                    foreach(var item in positionalIndex[srchtxtKey[0]])
                    {
                        res.Add(documents[item.Key].Text);
                    }
                }
                return Ok(res);
            }
            int i = CountSpacesAndNumericTemplates(srchtxt);
            while(i > 0) 
            {
                //TO DO
                i--;
            }
                

            return Ok(SearchWithDistance(srchtxtKey[0], srchtxtKey[1], distance));
        }
        List<string> SearchWithDistance(string word1, string word2, int distance)
        {
            // Implement the search logic here.
            List<string> result = new List<string>();

            if (!positionalIndex.ContainsKey(word1) || !positionalIndex.ContainsKey(word2))
            {
                return result;
            }

            foreach (var filePos1 in positionalIndex[word1])
            {
                int docId = filePos1.Key;
                List<int> positions1 = filePos1.Value;

                if (positionalIndex[word2].ContainsKey(docId))
                {
                    List<int> positions2 = positionalIndex[word2][docId];

                    foreach (int pos1 in positions1)
                    {
                        foreach (int pos2 in positions2)
                        {
                            if (Math.Abs(pos1 - pos2) == distance)
                            {
                                result.Add($"{docId}: Word '{word1}' at position {pos1}, Word '{word2}' at position {pos2}");
                            }
                        }
                    }
                }
            }

            return result;
        }
        int CountSpacesAndNumericTemplates(string input)
        {

            // Count occurrences of numeric templates using regex
            int templateCount = Regex.Matches(input, @"\/\d+").Count;

            return input.Count(Char.IsWhiteSpace) - templateCount;
        }
    }
}

