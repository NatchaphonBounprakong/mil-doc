using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WEB.API.DGA.MIL.DOC.TEST.DAL;
using WEB.API.DGA.MIL.DOC.TEST.Models;

namespace WEB.API.DGA.MIL.DOC.TEST.Services
{
    public class OrganizationServices
    {
        public Response GetOrganizationById(int id)
        {
            Response response = new Response();
            response.ApiName = "service/GetOrganizationById";

            try
            {
                using (DGAMilDocEntities context = new DGAMilDocEntities())
                {
                    var obj = context.Organization.Where(abc => abc.Id == id).FirstOrDefault();
                  
                    response.ResultData = obj;
                    response.Status = true;
                    response.Message = "Success";
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public Response GetListOrganization()
        {
            Response resp = new Response();

            return resp;
        }

        public Response AddOrganization(Organization organization)
        {
            Response resp = new Response();

            return resp;
        }
        public Response UpdateOrganization(Organization organization)
        {
            Response resp = new Response();

            return resp;
        }
        public Response DeleteOrganization(int id)
        {
            Response resp = new Response();

            return resp;
        }
    }
}