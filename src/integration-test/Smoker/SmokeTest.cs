using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Smoker
{
    // integration test for testing Helium (or any REST API)
    public class Test
    {
        private readonly List<Request> _requestList;
        private readonly string _baseUrl;

        private readonly HttpClient _client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });

        public Test(List<string> fileList, string baseUrl)
        {
            this._baseUrl = baseUrl;
            List<Request> list;
            List<Request> fullList = new List<Request>();
            _requestList = new List<Request>();

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

            _requestList = fullList.OrderBy(x => x.SortOrder).ThenBy(x => x.Index).ToList();
        }

        public Test()
        {
            // set timeout to 30 seconds
            _client.Timeout = new TimeSpan(0, 0, 30);
        }

        public async Task<bool> Run()
        {
            bool isError = false;
            DateTime dt;
            HttpRequestMessage req;
            string body;
            string res = string.Empty;

            // send the first request as a warm up
            //await Warmup(requestList[0].Url);

            // send each request
            foreach (Request r in _requestList)
            {
                try
                {
                    // create the request
                    using (req = new HttpRequestMessage(new HttpMethod(r.Verb), MakeUrl(r.Url)))
                    {
                        dt = DateTime.Now;

                        // process the response
                        using (HttpResponseMessage resp = await _client.SendAsync(req))
                        {
                            body = await resp.Content.ReadAsStringAsync();

                            Console.WriteLine($"{DateTime.Now.ToString("MM/dd hh:mm:ss")}\t{(int)resp.StatusCode}\t{(int)DateTime.Now.Subtract(dt).TotalMilliseconds}\t{resp.Content.Headers.ContentLength}\t{r.Url}");

                            // validate the response
                            if (resp.StatusCode == System.Net.HttpStatusCode.OK && r.Validation != null)
                            {
                                res = ValidateContentType(r, resp);
                                res += ValidateContentLength(r, resp);
                                res += ValidateContains(r, body);
                                res += ValidateJsonArray(r, body);
                                res += ValidateJsonObject(r, body);

                                if (!string.IsNullOrEmpty(res))
                                {
                                    Console.Write(res);
                                    res = string.Empty;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ignore any error and keep processing
                    Console.WriteLine($"{DateTime.Now.ToString("MM/dd hh:mm:ss")}\tException: {ex.Message}");
                    isError = true;
                }
            }

            return isError;
        }

        public async Task<string> RunFromWebRequest(int id)
        {
            DateTime dt;
            HttpRequestMessage req;
            string body;
            string res = string.Format($"Version: {Helium.Version.AssemblyVersion}\n\n");

            // send the first request as a warm up
            await Warmup(_requestList[0].Url);

            // send each request
            foreach (Request r in _requestList)
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
                            using (HttpResponseMessage resp = await _client.SendAsync(req))
                            {
                                body = await resp.Content.ReadAsStringAsync();

                                res += string.Format($"{DateTime.Now.ToString("MM/dd hh:mm:ss")}\t{(int)resp.StatusCode}\t{(int)DateTime.Now.Subtract(dt).TotalMilliseconds}\t{resp.Content.Headers.ContentLength}\t{r.Url}");

                                // validate the response
                                if (resp.StatusCode == System.Net.HttpStatusCode.OK && r.Validation != null)
                                {
                                    res += ValidateContentType(r, resp);
                                    res += ValidateContentLength(r, resp);
                                    res += ValidateContains(r, body);
                                    res += ValidateJsonArray(r, body);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore any error and keep processing
                        Console.WriteLine($"{ex.Message}\tException: {1}", DateTime.Now.ToString("MM/dd hh:mm:ss"));
                    }
                }
            }

            return res;
        }

        // run the tests
        public async Task RunLoop(int id, HeliumIntegrationTest.Config config, CancellationToken ct)
        {
            DateTime dt = DateTime.Now;
            HttpRequestMessage req;
            string body;
            string res;

            int i = 0;
            Request r;

            Random rand = new Random(DateTime.Now.Millisecond);

            if (ct.IsCancellationRequested)
            {
                return;
            }

            // send the first request as a warm up
            await Warmup(_requestList[0].Url);

            // loop forever
            while (true)
            {
                i = 0;

                // send each request
                while (i < _requestList.Count)
                {
                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }

                    if (config.Random)
                    {
                        i = rand.Next(0, _requestList.Count - 1);
                    }

                    r = _requestList[i];

                    try
                    {
                        // create the request
                        using (req = new HttpRequestMessage(new HttpMethod(r.Verb), MakeUrl(r.Url)))
                        {
                            dt = DateTime.Now;

                            // process the response
                            using (HttpResponseMessage resp = await _client.SendAsync(req))
                            {
                                body = await resp.Content.ReadAsStringAsync();
                                res = string.Empty;

                                // validate the response
                                if (resp.StatusCode == System.Net.HttpStatusCode.OK && r.Validation != null)
                                {
                                    res = ValidateStatusCode(r, resp);
                                    res = ValidateContentType(r, resp);
                                    res += ValidateContentLength(r, resp);
                                    res += ValidateContains(r, body);
                                    res += ValidateJsonArray(r, body);
                                }

                                // only log 4XX and 5XX status codes
                                if (config.Verbose || (int)resp.StatusCode > 399 || !string.IsNullOrEmpty(res))
                                {
                                    if (config.RunWeb)
                                    {
                                        // datetime is redundant for web app
                                        Console.WriteLine($"{id}\t{(int)resp.StatusCode}\t{(int)DateTime.Now.Subtract(dt).TotalMilliseconds}\t{resp.Content.Headers.ContentLength}\t{r.Url}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"{id}\t{DateTime.Now.ToString("MM/dd hh:mm:ss")}\t{(int)resp.StatusCode}\t{(int)DateTime.Now.Subtract(dt).TotalMilliseconds}\t{resp.Content.Headers.ContentLength}\t{r.Url}");
                                    }

                                    if (!string.IsNullOrEmpty(res))
                                    {
                                        // res has a LF appended, so don't use writeline
                                        Console.Write(res);
                                    }
                                }
                            }
                        }
                    }
                    catch (System.Threading.Tasks.TaskCanceledException tce)
                    {
                        // request timeout error

                        string message = tce.Message;

                        if (tce.InnerException != null)
                        {
                            message = tce.InnerException.Message;
                        }

                        Console.WriteLine($"{id}\t500\t{(int)DateTime.Now.Subtract(dt).TotalMilliseconds}\t0\t{r.Url}\tSmokerException\t{message}");
                    }
                    catch (Exception ex)
                    {
                        // ignore any error and keep processing
                        Console.WriteLine($"{id}\t500\t{(int)DateTime.Now.Subtract(dt).TotalMilliseconds}\t0\t{r.Url}\tSmokerException\t{ex.Message}\n{ex}");
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
                    using (var resp = await _client.SendAsync(req))
                    {
                        string body = await resp.Content.ReadAsStringAsync();
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Warmup Failed: {ex.Message}");
            }
        }

        // validate the status code
        public string ValidateStatusCode(Request r, HttpResponseMessage resp)
        {
            string res = string.Empty;

            if ((int)resp.StatusCode == r.Validation.Code)
            {
                res += string.Format($"\tValidation Failed: StatusCode: {(int)resp.StatusCode} Expected: {r.Validation.Code}\n");
            }

            return res;
        }


        // validate the content type header if specified in the test
        public string ValidateContentType(Request r, HttpResponseMessage resp)
        {
            string res = string.Empty;

            if (!string.IsNullOrEmpty(r.ContentType))
            {
                if (!resp.Content.Headers.ContentType.ToString().StartsWith(r.Validation.ContentType))
                {
                    res += string.Format($"\tValidation Failed: ContentType: {resp.Content.Headers.ContentType}\n");
                }
            }

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
                    res = string.Format($"\tValidation Failed: MinContentLength: {resp.Content.Headers.ContentLength}\n");
                }
            }

            // validate the content max length if specified in test
            if (r.Validation.MaxLength > 0)
            {
                if (resp.Content.Headers.ContentLength > r.Validation.MaxLength)
                {
                    res += string.Format($"\tValidation Failed: MaxContentLength: {resp.Content.Headers.ContentLength}\n");
                }
            }

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
                        res += string.Format($"\tValidation Failed: Contains: {c.Value.PadRight(40).Substring(0, 40).Trim()}\n");
                    }
                }
            }

            return res;
        }

        // run json array validation rules
        public string ValidateJsonArray(Request r, string body)
        {
            string res = string.Empty;

            if (r.Validation.JsonArray != null)
            {
                if (r.Validation.JsonArray.MinLength > 0 || r.Validation.JsonArray.MaxLength > 0)
                {
                    try
                    {
                        // deserialize the json
                        var resList = JsonConvert.DeserializeObject<List<dynamic>>(body) as List<dynamic>;

                        // validate min length
                        if (r.Validation.JsonArray.MinLength > 0 && r.Validation.JsonArray.MinLength > resList.Count)
                        {
                            res += string.Format($"\tValidation Failed: MinJsonCount: {resList.Count}\n");
                        }

                        // validate max length
                        if (r.Validation.JsonArray.MaxLength > 0 && r.Validation.JsonArray.MaxLength < resList.Count)
                        {
                            res += string.Format($"\tValidation Failed: MaxJsonCount: {resList.Count}\n");
                        }
                    }
                    catch (SerializationException se)
                    {
                        res += string.Format($"Exception|{se.Source}|{se.TargetSite}|{se.Message}");
                    }

                    catch (Exception ex)
                    {
                        res += string.Format($"Exception|{ex.Source}|{ex.TargetSite}|{ex.Message}");
                    }
                }
            }

            return res;
        }

        // run json object validation rules
        public string ValidateJsonObject(Request r, string body)
        {
            string res = string.Empty;

            if (r.Validation.JsonObject != null && r.Validation.JsonObject.Count > 0)
            {
                try
                {
                    // deserialize the json into an IDictionary
                    IDictionary<string, object> dict = JsonConvert.DeserializeObject<ExpandoObject>(body);

                    foreach (var f in r.Validation.JsonObject)
                    {
                        if (!string.IsNullOrEmpty(f.Field) && dict.ContainsKey(f.Field))
                        {
                            // null values check for the existance of the field in the payload
                            // used when values are not known
                            if (f.Value != null && !dict[f.Field].Equals(f.Value))
                            {
                                res += string.Format($"\tValidation Failed: {f.Field}: {f.Value} : Expected: {dict[f.Field]}\n");
                            }
                        }
                        else
                        {
                            res += string.Format($"\tValidation Failed: Field Not Found: {f.Field}\n");
                        }
                    }


                }
                catch (SerializationException se)
                {
                    res += string.Format($"Exception|{se.Source}|{se.TargetSite}|{se.Message}");
                }

                catch (Exception ex)
                {
                    res += string.Format($"Exception|{ex.Source}|{ex.TargetSite}|{ex.Message}");
                }
            }

            return res;
        }

        // read json test file
        public List<Request> ReadJson(string file)
        {
            // check for file exists
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
            {
                Console.WriteLine($"File Not Found: {file}");
                return null;
            }

            // read the file
            string json = File.ReadAllText(file);

            // check for empty file
            if (string.IsNullOrEmpty(json))
            {
                Console.WriteLine($"Unable to read file {file}");
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
            string url = _baseUrl;

            // avoid // in the URL
            if (url.EndsWith("/") && path.StartsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url + path;
        }
    }
}
