using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Data.Entity.Core.Objects;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SendGrid;
using SendGrid.Helpers.Mail;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.API;
using System.Web.Http.Results;

namespace AngularJSAuthentication.API
{
    [RoutePrefix("api/Customer")]
    public class CustomerController : ApiController
    {
        CampaignEntities ctx = new CampaignEntities();
        private AuthRepository _repo = null;
        private CampaignEntities _repo2 = null;

        public CustomerController()
        {
            _repo = new AuthRepository();
            _repo2 = new CampaignEntities();
        }


        // GET api/<controller>
        [HttpGet]
        public IEnumerable<usp_getQualifiedPassdown_Result> Get(int id)
        {
            return ctx.usp_getQualifiedPassdown(id).AsEnumerable();
        }

        // // GET api/<controller>/5
        // public IEnumerable<string> Get(int id)
        // {

        // }


        // POST api/<controller>
        public async Task<IHttpActionResult> Enroll([FromBody]Campaign customer)//void Post([FromBody]string value) //async Task<IHttpActionResult> Register(UserModel userModel)
        {
            //Campaign cmpn = new Campaign() { ID = customer.ID, PrntID = customer.PrntID, Phone = customer.Phone, Email = customer.Email };
            //var cvid = Int32.Parse(externalLogin.Viid);

            //ObjectResult objRslt = ctx.usp_insertCustomer(customer.ID,customer.Name,customer.PrntID,customer.PassPrnt,customer.Email,customer.Phone);

            var check1 = _repo2.Campaigns.SingleOrDefault(email => email.Email == customer.Email);
            if (check1 != null)
            {
                var Content = "This Vi email address " + customer.Email.ToString() + " already exists in the database.";
                //};
                return BadRequest(Content);
            }

            var check = _repo2.Campaigns.Find(customer.ID);
            if (check == null)
            {
                ObjectResult objRslt = ctx.usp_insertCustomer(customer.ID, customer.Name, customer.PrntID, customer.PassPrnt, customer.Email, customer.Phone);
                var check2 = _repo2.Campaigns.Find(customer.ID);
                if(check2 != null)
                {
                    var Content = "Success!";
                    UserModel um = new UserModel();
                    um.UserName = customer.Email;
                    um.Password = customer.ID.ToString();
                    um.ConfirmPassword = customer.ID.ToString();
                    IdentityResult result = await _repo.RegisterUser(um);

                    IdentityUser user = await _repo.FindUser(um.UserName, um.Password);
                    user.Email = check2.Email;
                    user.EmailConfirmed = true;
                    result = await _repo.UpdateUser(user);

                    var check3 = _repo2.Campaigns.Find(customer.PrntID);
                    if (check3 != null)
                    {
                        check3.TokenID = "";
                    }

                    _repo2.SaveChanges();

                    await SendSG(customer);
                    return Ok(Content);
                }
                else
                {
                    var Content = "This Vi customer ID " + customer.ID.ToString() + " did not save to the database.";
                    //};
                    return BadRequest(Content);
                }

                //customer.StartDate = DateTime.Now;
                //customer.Rank = "Customer";
                //Campaign success = _repo2.Campaigns.Add(customer);
                //_repo2.SaveChanges();

                //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, customer);

                //return Ok();
            }
            else
            {
                //var message = new HttpResponseMessage(HttpStatusCode.BadRequest)
                //{
                var Content = "This Vi customer ID " + customer.ID.ToString() + " already exists in the database.";
                //};
                return BadRequest(Content);
                //throw new HttpResponseException(message);
                //return new HttpResponseMessage(HttpStatusCode.NotModified);
            } 
            //return Ok();
        }


        static async Task SendSG(Campaign customer)
        {
            string apiKey = ConfigurationManager.AppSettings["SendGridKey"]; //Environment.GetEnvironmentVariable("NAME_OF_THE_ENVIRONMENT_VARIABLE_FOR_YOUR_SENDGRID_KEY", EnvironmentVariableTarget.User);
            dynamic sg = new SendGridAPIClient(apiKey);

            Email from = new Email("therebelrun@gmail.com");
            String subject = "Welcome to the Rebel Run!!";
            Email to = new Email("jlpatton@outlook.com");// customer.Email);
            Content content = new Content("text/html", "This is the ViID: " + customer.ID.ToString());
            Mail mail = new Mail(from, subject, to, content);

            mail.TemplateId = "f7ddfe1c-5fcf-41b6-ae25-d71b4f2128b6";
            mail.Personalization[0].AddSubstitution("-name-", customer.Name);
            mail.Personalization[0].AddSubstitution("-city-", "Denver");
            mail.Personalization[0].AddSubstitution("-viid-", customer.ID.ToString());

            dynamic response = await sg.client.mail.send.post(requestBody: mail.Get());
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
            var check2 = _repo2.Campaigns.Find(id);
            if (check2 != null)
                {
                    check2.TokenID = value;
                    _repo2.SaveChanges();
                    //return Ok();
                }
                else
                {
                    var Content = "This Vi customer ID " + id.ToString() + " did not exist in the database. HeldPassdown did not update";
                    //};
                    //return BadRequest(Content);
                }
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
                _repo2.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}