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
                return BadRequest("Search text cannot be empt.");

            var srchtxtKey = Split(srchtxt);

            if (Regex.IsMatch(srchtxtKey[0], @"\/\d+"))
                return BadRequest($"The search text: '{srchtxt}' cannot start with '{srchtxtKey[0]}'.");
            List<int> res = null;
            if (srchtxtKey.Count() == 1)
            {


                if (positionalIndex.ContainsKey(srchtxtKey[0]))
                {
                    foreach (var item in positionalIndex[srchtxtKey[0]])
                    {
                        res.Add(item.Key);
                    }
                }
                return Ok(res == null ? new List<int>() : res);
            }

            for (int i = 0; i <= srchtxtKey.Count - 3; i += 2)
            {


                if (res == null)
                {
                    // If result is null, initialize it with the document IDs from the first term
                    int numericValue = int.Parse(Regex.Match(srchtxtKey[i + 1], @"\d+").Value);
                    res = SearchWithDistance(srchtxtKey[i], srchtxtKey[i + 2], numericValue);
                }
                else
                {
                    // Find the intersection of the current result and the document IDs for the current term
                    int numericValue = int.Parse(Regex.Match(srchtxtKey[i + 1], @"\d+").Value);
                    res = Intersect<int>(res, SearchWithDistance(srchtxtKey[i], srchtxtKey[i + 2], numericValue));
                    if (res.Count() == 0)
                    {
                        // If a term is not found, return a bad request with the missing term
                        return BadRequest($"No occurrences of '{srchtxt}' found.");
                    }
                }
            }
            return Ok(res);

        }
        List<int> SearchWithDistance(string word1, string word2, int distance)
        {
            List<int> result = new List<int>();

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
                                result.Add(docId);
                            }
                        }
                    }
                }
            }

            return result;
        }
        public static List<T> Intersect<T>(List<T> list1, List<T> list2)
        {
            var set = new HashSet<T>(list2);
            var result = new List<T>();

            foreach (var item in list1)
                if (set.Contains(item))
                    result.Add(item);

            return result;
        }
        List<string> Split(string s)
        {
            List<string> result = new List<string>();
            var srchtxtKey = s.ToLower().Split(new[] { ' ', '.', '!', '?', ',' }, StringSplitOptions.RemoveEmptyEntries);

            result.Add(srchtxtKey[0]);
            for (int i = 1; i< srchtxtKey.Count(); i++)
            {
                if (Regex.IsMatch(srchtxtKey[i], @"\/\d+"))
                {
                    // If the part is a numeric template, add it to the result array
                    result.Add(srchtxtKey[i]);
                }
                else 
                {
                    if(!Regex.IsMatch(srchtxtKey[i - 1], @"\/\d+"))
                    {
                        result.Add("/1");
                        result.Add(srchtxtKey[i]);

                    }
                    else
                    {
                        result.Add(srchtxtKey[i]);
                    }
                    

                }
            }
            return result;
        }
    }
}

