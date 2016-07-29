using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Utils;

namespace Engine.Cloud.Core.Utils
{
    public class SendEmailClient
    {
        private readonly string _mailServer = AppSettings.GetString("SendEmail.Server"); //mail.tecla.com.br
        private readonly string _password = AppSettings.GetString("SendEmail.Password"); //cl0udn0r3p4y
        private readonly string _userName = AppSettings.GetString("SendEmail.User"); //noreply@cloud.tecla.com.br
        private readonly string _from = AppSettings.GetString("SendEmail.From"); //noreply@cloud.tecla.com.br

        public SendEmailClient(string from)
        {
            _from = from;
        }

        public SendEmailClient()
        {
            this.Params = new Dictionary<string, string>();
            this.Bcc = new List<string>();
        }

        public string To { get; set; }
        public List<string> Bcc { get; set; }
        public string Subject { get; set; }

        private string _template;
        public string Template
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + "\\" + _template; }
            set { this._template = value; }
        }

        public Dictionary<string, string> Params { get; set; }

        public void Send()
        {
            try
            {
                var mailMessage = CreateMailMessage();

                var smtpClient = new SmtpClient(_mailServer, 587)
                {
                    Credentials = new System.Net.NetworkCredential(_userName, _password)
                };

                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                LogFactory.GetInstance().Log(ex);
                throw;
            }
        }

        private MailMessage CreateMailMessage()
        {
            var template = new MailDefinition { BodyFileName = this.Template, From = _from };
            MailMessage message = template.CreateMailMessage(To.Replace(';', ','), this.Params, new LiteralControl());

            if (this.Bcc.Any())
                this.Bcc.ForEach(email => message.Bcc.Add(new MailAddress(email)));
                
            message.IsBodyHtml = true;

            message.Subject = this.Subject;

            return message;
        }
    }
}
