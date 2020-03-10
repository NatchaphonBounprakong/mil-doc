using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WEB.API.DGA.MIL.DOC.DataAccesLayer;

namespace WEB.API.DGA.MIL.DOC.Services
{

    public class UserServices
    {
        public Response resp = new Response();

        public Response Login(string username, string password)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var user = ctx.User.Include("Organization")
                        .Where(o => o.Username == username && o.Password == password)
                        .FirstOrDefault();

                    user.Expire = DateTime.Now.AddDays(1);
                    ctx.SaveChanges();
                    user.Password = null;
                    user.Organization.User = null;
                    if (user != null)
                    {
                        resp.ResponseObject = user;
                        resp.Status = true;
                    }
                    else
                    {
                        resp.Status = false;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }



    }
}