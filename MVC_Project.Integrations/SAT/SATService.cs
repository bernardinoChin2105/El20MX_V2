using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.SAT
{
    public class SATService
    {
        public static string GetCredentialStatusSat(string credentialId, string provider)
        {
            var SATResponse = SATwsService.GetCredentialSat(credentialId);

            if (SATResponse.status == "valid")
                return SystemStatus.ACTIVE.ToString();
            else
                return SystemStatus.INACTIVE.ToString();
        }

        public static CredentialsResponse CreateCredential(CredentialRequest request, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                var loginSat = new LogInSATModel { rfc = request.rfc, password = request.ciec, type = "ciec" };
                var satModel = SATwsService.CreateCredentialSat(loginSat);

                return new CredentialsResponse { id = satModel.id, status = satModel.status };
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static CredentialsResponse CreateCredentialEfirma(string cer, string key, string password, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                var loginSat = new EfirmaModel { certificate = cer, privateKey=key, password = password, type = "efirma" };
                var satModel = SATwsService.CreateCredentialEfirma(loginSat);

                return new CredentialsResponse { id = satModel.id, status = satModel.status };
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static CertificateResponse CreateCertificates(string cer, string key, string password, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                var loginSat = new CertificateModel { certificate = cer, privateKey = key, password = password, type = "csd" };
                var satModel = SATwsService.CreateCertificates(loginSat);

                return new CertificateResponse { id = satModel.id };
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static void DeleteCertificates(string id, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                SATwsService.DeleteCertificates(id);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static string GenerateExtractions(string rfc, DateTime dateOnStart, DateTime dateOnEnd, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                var response = SATwsService.GenerateExtractions(rfc, dateOnStart, dateOnEnd);
                return response.id;
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static void GenerateTaxStatus(string rfc, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                SATwsService.GenerateTaxStatus(rfc);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static InvoicesModel GetInvoicesByExtractions(string rfc, string from, string to, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                return SATwsService.GetInvoicesByExtractions(rfc, from, to);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static List<InvoicesCFDI> GetCFDIs(List<string> uuids, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                return SATwsService.GetInvoicesCFDI(uuids);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static InvoicesModel GetTaxStatus(string rfc, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                return SATwsService.GetTaxStatus(rfc);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static InvoicesInfo PostIssueIncomeInvoices(dynamic invoice,string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                return SATwsService.PostIssueIncomeInvoices(invoice);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static InvoicesInfo PostIssuePaymentInvoices(dynamic invoice, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                return SATwsService.PostIssuePaymentInvoices(invoice);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        //Para CFDI relacionados(devoluciones o cancelaciones)
        public static InvoicesInfo PostIssueRefundInvoices(dynamic invoice, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                return SATwsService.PostRefundInvoices(invoice);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        //Método para Eliminar la credencial
        public static string DeleteCredential(string id, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                return SATwsService.DeleteCredential(id);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }
    }
}
