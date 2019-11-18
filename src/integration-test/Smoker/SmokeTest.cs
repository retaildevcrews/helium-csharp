using Helium;
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
                        dt = DateTime.UtcNow;

                        // process the response
                        using (HttpResponseMessage resp = await _client.SendAsync(req))
                        {
                            body = await resp.Content.ReadAsStringAsync();

                            Console.WriteLine($"{DateTime.UtcNow.ToString("MM/dd hh:mm:ss")}\t{(int)resp.StatusCode}\t{(int)DateTime.UtcNow.Subtract(dt).TotalMilliseconds}\t{resp.Content.Headers.ContentLength}\t{r.Url}");

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
                    Console.WriteLine($"{DateTime.UtcNow.ToString("MM/dd hh:mm:ss")}\tException: {ex.Message}");
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

            var reqList = ReadJson("TestFiles/baseline.json");

            if (reqList == null || reqList.Count == 0)
            {
                return "Unable to read baseline.json";
            }

            // send each request
            foreach (Request r in reqList)
            {
                try
                {
                    // create the request
                    using (req = new HttpRequestMessage(new HttpMethod(r.Verb), MakeUrl(r.Url)))
                    {
                        dt = DateTime.UtcNow;

                        // process the response
                        using (HttpResponseMessage resp = await _client.SendAsync(req))
                        {
                            body = await resp.Content.ReadAsStringAsync();

                            res += string.Format($"{DateTime.UtcNow.ToString("MM/dd hh:mm:ss")}\t{(int)resp.StatusCode}\t{(int)DateTime.UtcNow.Subtract(dt).TotalMilliseconds}\t{resp.Content.Headers.ContentLength}\t{r.Url}\n");

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
                    Console.WriteLine($"{ex.Message}\tException: {1}", DateTime.UtcNow.ToString("MM/dd hh:mm:ss"));
                }
            }

            return res;
        }

        // run the tests
        public async Task RunLoop(int id, Helium.Config config, CancellationToken ct)
        {
            DateTime dt = DateTime.UtcNow;
            DateTime nextPrune = DateTime.UtcNow.AddMinutes(1);
            DateTime dtMax = DateTime.MaxValue;
            HttpRequestMessage req;
            string body;
            string res;

            int i = 0;
            Request r;

            Random rand = new Random(DateTime.UtcNow.Millisecond);

            // only run for duration (seconds)
            if (config.Duration > 0)
            {
                dtMax = DateTime.UtcNow.AddSeconds(config.Duration);
            }

            if (ct.IsCancellationRequested)
            {
                return;
            }

            // loop for duration or forever
            while (DateTime.UtcNow < dtMax)
            {
                i = 0;

                // send each request
                while (i < _requestList.Count && DateTime.UtcNow < dtMax)
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
                            dt = DateTime.UtcNow;

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

                                int duration = (int)DateTime.UtcNow.Subtract(dt).TotalMilliseconds;

                                // only log 4XX and 5XX status codes
                                if (config.Verbose || (int)resp.StatusCode > 399 || !string.IsNullOrEmpty(res))
                                {
                                    if (config.RunWeb)
                                    {
                                        // datetime is redundant for web app
                                        Console.WriteLine($"{id}\t{(int)resp.StatusCode}\t{duration}\t{resp.Content.Headers.ContentLength}\t{r.Url}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"{id}\t{DateTime.UtcNow.ToString("MM/dd hh:mm:ss")}\t{duration}\t{resp.Content.Headers.ContentLength}\t{r.Url}");
                                    }

                                    if (!string.IsNullOrEmpty(res))
                                    {
                                        // res has a LF appended, so don't use writeline
                                        Console.Write(res);
                                    }
                                }

                                App.Metrics.Add((int)resp.StatusCode, duration);
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

                        Console.WriteLine($"{id}\t500\t{(int)DateTime.UtcNow.Subtract(dt).TotalMilliseconds}\t0\t{r.Url}\tSmokerException\t{message}");
                    }
                    catch (Exception ex)
                    {
                        // ignore any error and keep processing
                        Console.WriteLine($"{id}\t500\t{(int)DateTime.UtcNow.Subtract(dt).TotalMilliseconds}\t0\t{r.Url}\tSmokerException\t{ex.Message}\n{ex}");
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

                    if (DateTime.UtcNow > nextPrune)
                    {
                        App.Metrics.Prune();
                        nextPrune = DateTime.UtcNow.AddMinutes(1);
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

            if ((int)resp.StatusCode != r.Validation.Code)
            {
                res += string.Format($"\tValidation Failed: StatusCode: {(int)resp.StatusCode} Expected: {r.Validation.Code}\n");
                App.Metrics.Add(0, 0);
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
                    App.Metrics.Add(0, 0);
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
                    App.Metrics.Add(0, 0);
                }
            }

            // validate the content max length if specified in test
            if (r.Validation.MaxLength > 0)
            {
                if (resp.Content.Headers.ContentLength > r.Validation.MaxLength)
                {
                    res += string.Format($"\tValidation Failed: MaxContentLength: {resp.Content.Headers.ContentLength}\n");
                    App.Metrics.Add(0, 0);
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
                        App.Metrics.Add(0, 0);
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
                try
                {
                    // deserialize the json
                    var resList = JsonConvert.DeserializeObject<List<dynamic>>(body) as List<dynamic>;

                    // validate count
                    if (r.Validation.JsonArray.Count > 0 && r.Validation.JsonArray.Count != resList.Count)
                    {
                        res += string.Format($"\tValidation Failed: JsonCount: {r.Validation.JsonArray.Count}  Actual: {resList.Count}\n");
                        App.Metrics.Add(0, 0);
                    }

                    // validate count is zero
                    if (r.Validation.JsonArray.CountIsZero && 0 != resList.Count)
                    {
                        res += string.Format($"\tValidation Failed: JsonCountIsZero: Actual: {resList.Count}\n");
                        App.Metrics.Add(0, 0);
                    }

                    // validate min count
                    if (r.Validation.JsonArray.MinCount > 0 && r.Validation.JsonArray.MinCount > resList.Count)
                    {
                        res += string.Format($"\tValidation Failed: MinJsonCount: {r.Validation.JsonArray.MinCount}  Actual: {resList.Count}\n");
                        App.Metrics.Add(0, 0);
                    }

                    // validate max count
                    if (r.Validation.JsonArray.MaxCount > 0 && r.Validation.JsonArray.MaxCount < resList.Count)
                    {
                        res += string.Format($"\tValidation Failed: MaxJsonCount: {r.Validation.JsonArray.MaxCount}  Actual: {resList.Count}\n");
                        App.Metrics.Add(0, 0);
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
