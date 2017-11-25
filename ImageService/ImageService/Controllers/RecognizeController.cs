using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Cognitive.CustomVision;
using Microsoft.Cognitive.CustomVision.Models;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;

namespace ImageService.Controllers
{
    public class RecognizeController : ApiController
    {
        
        // POST api/values
        public async Task<HttpResponseMessage> Post()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            try
            {
                // Get the prediction key, which is used in place of the training key when making predictions
                string root = HttpContext.Current.Server.MapPath("~/App_Data");
                var provider = new MultipartFormDataStreamProvider(root);
                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);
                string key = provider.FormData.Get("key");
                Guid projectid = new Guid("57471653-6e79-455f-b874-ee00d1014c37");

                // Create a prediction endpoint, passing in a prediction credentials object that contains the obtained prediction key
                
                PredictionEndpointCredentials predictionEndpointCredentials = new PredictionEndpointCredentials(key);
                PredictionEndpoint endpoint = new PredictionEndpoint(predictionEndpointCredentials);
                                
                MemoryStream memStream = new MemoryStream(File.ReadAllBytes(provider.FileData[0].LocalFileName));

                // Make a prediction against the new project
                Debug.WriteLine("Making a prediction:");
                var result = endpoint.PredictImage(projectid, memStream);
                               
                // Loop over each prediction and write out the results
                var predicition = result.Predictions.FirstOrDefault(P => P.Probability >= 0.8);
                if( predicition == null )
                    return Request.CreateResponse(HttpStatusCode.OK, "Could not find a good match for the uploaded image. Please try again.");
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Your image is of type " + predicition.Tag + " with the probability of " + predicition.Probability);
                }
                //return Request.CreateResponse<ImagePredictionResultModel>(HttpStatusCode.OK, result);
            }
            catch (System.Exception e)
            {
                Debug.WriteLine("Error in predicition! Exception: " + e.Message);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}
