using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Smoker
{
    // integration test for testing Helium (or any REST API)
    public class Test
    {
        private readonly List<Request> requestList;
        private readonly string baseUrl;

        private readonly HttpClient client = new HttpClient();

        public Test(List<string> fileList, string baseUrl)
        {
            this.baseUrl = baseUrl;
            List<Request> list;
            List<Request> fullList = new List<Request>();

            requestList = new List<Request>();

            foreach (string inputFile in fileList)
            {
                // read the json file
                list = ReadJson(inputFile);

                if (list != null && list.Count > 0)
                {
                    fullList.AddRange(list);
                }
            }

            // exit if can't read the json file
            if (fullList == null || fullList.Count == 0)
            {
                throw new FileLoadException("Unable to read input files");
            }

            requestList = fullList.OrderBy(x => x.SortOrder).ThenBy(x => x.Index).ToList();
        }

        public async Task<bool> Run()
        {
            bool isError = false;
            DateTime dt;
            HttpRequestMessage req;
            string body;

            // send the first request as a warm up
            await Warmup(requestList[0].Url);

            // send each request
            foreach (Request r in requestList)
            {
                if (r.IsBaseTest)
                {
                    try
                    {
                        // create the request
                        using (req = new HttpRequestMessage(new HttpMethod(r.Verb), MakeUrl(r.Url)))
                        {
                            dt = DateTime.Now;

                            // process the response
                            using (HttpResponseMessage resp = await client.SendAsync(req))
                            {
                                body = await resp.Content.ReadAsStringAsync();

                                Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", DateTime.Now.ToString("MM/dd hh:mm:ss"), (int)resp.StatusCode, (int)DateTime.Now.Subtract(dt).TotalMilliseconds, resp.Content.Headers.ContentLength, r.Url);

                                // validate the response
                                if (r.Validation != null)
                                {
                                    ValidateContentType(r, resp);
                                    ValidateContentLength(r, resp);
                                    ValidateContains(r, body);
                                    ValidateJson(r, body);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore any error and keep processing
                        Console.WriteLine("{0}\tException: {1}", DateTime.Now.ToString("MM/dd hh:mm:ss"), ex.Message);
                        isError = true;
                    }
                }
            }

            return isError;
        }

        public async Task<string> RunFromWebRequest(int id)
        {
            DateTime dt;
            HttpRequestMessage req;
            string body;
            string res = string.Empty;

            // send the first request as a warm up
            await Warmup(requestList[0].Url);

            // send each request
            foreach (Request r in requestList)
            {
                if (r.IsBaseTest)
                {
                    try
                    {
                        // create the request
                        using (req = new HttpRequestMessage(new HttpMethod(r.Verb), MakeUrl(r.Url)))
                        {
                            dt = DateTime.Now;

                            // process the response
                            using (HttpResponseMessage resp = await client.SendAsync(req))
                            {
                                body = await resp.Content.ReadAsStringAsync();

                                Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", id, (int)resp.StatusCode, (int)DateTime.Now.Subtract(dt).TotalMilliseconds, resp.Content.Headers.ContentLength, r.Url);

                                res += string.Format("{0}\t{1}\t{2}\t{3}\t{4}\r\n", DateTime.Now.ToString("MM/dd hh:mm:ss"), (int)resp.StatusCode, (int)DateTime.Now.Subtract(dt).TotalMilliseconds, resp.Content.Headers.ContentLength, r.Url);

                                // validate the response
                                if (r.Validation != null)
                                {
                                    res += ValidateContentType(r, resp);
                                    res += ValidateContentLength(r, resp);
                                    res += ValidateContains(r, body);
                                    res += ValidateJson(r, body);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore any error and keep processing
                        Console.WriteLine("{0}\tException: {1}", DateTime.Now.ToString("MM/dd hh:mm:ss"), ex.Message);
                    }
                }
            }

            return res;
        }

        // run the tests
        public async Task RunLoop(int id, HeliumIntegrationTest.Config config, CancellationToken ct)
        {
            DateTime dt;
            HttpRequestMessage req;
            string body;

            int i = 0;
            Request r;

            Random rand = new Random(DateTime.Now.Millisecond);

            if (ct.IsCancellationRequested)
            {
                return;
            }

            // send the first request as a warm up
            await Warmup(requestList[0].Url);

            // loop forever
            while (true)
            {
                i = 0;

                // send each request
                while (i < requestList.Count)
                {
                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }

                    if (config.Random)
                    {
                        i = rand.Next(0, requestList.Count - 1);
                    }

                    r = requestList[i];

                    try
                    {
                        // create the request
                        using (req = new HttpRequestMessage(new HttpMethod(r.Verb), MakeUrl(r.Url)))
                        {
                            dt = DateTime.Now;

                            // process the response
                            using (HttpResponseMessage resp = await client.SendAsync(req))
                            {
                                body = await resp.Content.ReadAsStringAsync();

                                // datetime is redundant for web app
                                if (config.RunWeb)
                                {
                                    Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", id, (int)resp.StatusCode, (int)DateTime.Now.Subtract(dt).TotalMilliseconds, resp.Content.Headers.ContentLength, r.Url);
                                }
                                else
                                {
                                    Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", id, DateTime.Now.ToString("MM/dd hh:mm:ss"), (int)resp.StatusCode, (int)DateTime.Now.Subtract(dt).TotalMilliseconds, resp.Content.Headers.ContentLength, r.Url);
                                }

                                // validate the response
                                if (r.Validation != null)
                                {
                                    ValidateContentType(r, resp);
                                    ValidateContentLength(r, resp);
                                    ValidateContains(r, body);
                                    ValidateJson(r, body);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore any error and keep processing
                        Console.WriteLine("{0}\tException: {1}", DateTime.Now.ToString("MM/dd hh:mm:ss"), ex.Message);
                    }

                    // increment the index
                    if (!config.Random)
                    {
                        i++;
                    }

                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }

                    // sleep between each request
                    System.Threading.Thread.Sleep(config.SleepMs);

                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }
        }

        // send the first request in the list as a warm up request
        // results are not displayed
        public async Task Warmup(string path)
        {
            try
            {
                using (var req = new HttpRequestMessage(new HttpMethod("GET"), MakeUrl(path)))
                {
                    using (var resp = await client.SendAsync(req))
                    {
                        string body = await resp.Content.ReadAsStringAsync();
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Warmup Failed: {0}", ex.Message);
            }
        }

        // validate the content type header if specified in the test
        public string ValidateContentType(Request r, HttpResponseMessage resp)
        {
            string res = string.Empty;

            if (!string.IsNullOrEmpty(r.ContentType))
            {
                if (!resp.Content.Headers.ContentType.ToString().StartsWith(r.Validation.ContentType))
                {
                    res += string.Format("\tValidation Failed: ContentType: {0}\r\n", resp.Content.Headers.ContentType);
                }
            }

            Console.Write(res);

            return res;
        }

        // validate the content length range if specified in test
        public string ValidateContentLength(Request r, HttpResponseMessage resp)
        {
            string res = string.Empty;

            // validate the content min length if specified in test
            if (r.Validation.MinLength > 0)
            {
                if (resp.Content.Headers.ContentLength < r.Validation.MinLength)
                {
                    res = string.Format("\tValidation Failed: MinContentLength: {0}\r\n", resp.Content.Headers.ContentLength);
                }
            }

            // validate the content max length if specified in test
            if (r.Validation.MaxLength > 0)
            {
                if (resp.Content.Headers.ContentLength > r.Validation.MaxLength)
                {
                    res += string.Format("\tValidation Failed: MaxContentLength: {0}\r\n", resp.Content.Headers.ContentLength);
                }
            }

            Console.Write(res);

            return res;
        }

        // validate the contains rules
        public string ValidateContains(Request r, string body)
        {
            string res = string.Empty;

            if (!string.IsNullOrEmpty(body) && r.Validation.Contains != null && r.Validation.Contains.Count > 0)
            {
                // validate each rule
                foreach (var c in r.Validation.Contains)
                {
                    // compare values
                    if (!body.Contains(c.Value, c.IsCaseSensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
                    {
                        res += string.Format("\tValidation Failed: Contains: {0}\r\n", c.Value.PadRight(40).Substring(0, 40).Trim());
                    }
                }
            }

            Console.Write(res);

            return res;
        }

        // run json validation rules
        public string ValidateJson(Request r, string body)
        {
            string res = string.Empty;

            if (r.Validation.Json != null)
            {
                if (r.Validation.Json.MinLength > 0 || r.Validation.Json.MaxLength > 0)
                {
                    // deserialize the json
                    var serializer = new DataContractJsonSerializer(typeof(List<dynamic>));
                    var resList = serializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(body))) as List<dynamic>;

                    // validate min length
                    if (r.Validation.Json.MinLength > 0 && r.Validation.Json.MinLength > resList.Count)
                    {
                        res += string.Format("\tValidation Failed: MinJsonCount: {0}\r\n", resList.Count);
                    }

                    // validate max length
                    if (r.Validation.Json.MaxLength > 0 && r.Validation.Json.MaxLength < resList.Count)
                    {
                        res += string.Format("\tValidation Failed: MaxJsonCount: {0}\r\n", resList.Count);
                    }
                }
            }

            Console.Write(res);

            return res;
        }

        // read json test file
        public List<Request> ReadJson(string file)
        {
            // check for file exists
            if (!File.Exists(file))
            {
                Console.WriteLine("File Not Found: {0}", file);
                return null;
            }

            // read the file
            string json = File.ReadAllText(file);

            // check for empty file
            if (string.IsNullOrEmpty(json))
            {
                Console.WriteLine("Unable to read file {0}", file);
                return null;
            }

            try
            {
                // deserialize json into a list (array)
                List<Request> list = JsonConvert.DeserializeObject<List<Request>>(json);

                if (list != null && list.Count > 0)
                {
                    List<Request> l2 = new List<Request>();

                    foreach (Request r in list)
                    {
                        r.Index = l2.Count;
                        l2.Add(r);
                    }
                    // success
                    return l2.OrderBy(x => x.SortOrder).ThenBy(x => x.Index).ToList();
                }

                Console.WriteLine("Invalid JSON file");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // couldn't read the list
            return null;
        }

        // build the URL from the base url and path in the test file
        private string MakeUrl(string path)
        {
            string url = baseUrl;

            // avoid // in the URL
            if (url.EndsWith("/") && path.StartsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url + path;
        }
    }
}
