using System;
using System.Net.Mail;

namespace Flubu.Tasks.Misc
{
    /// <summary>
    /// Sends an email.
    /// </summary>
    public class SendMailTask : TaskBase
    {
        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get
            {
                return String.Format(
                    System.Globalization.CultureInfo.InvariantCulture, 
                    "Send mail to '{0}' with subject '{1}'", 
                    to, 
                    subject);
            }
        }

        /// <summary>
        /// Gets or sets the SMTP server to use.
        /// </summary>
        /// <value>The SMTP server.</value>
        public string SmtpServer
        {
            get { return smtpServer; }
            set { smtpServer = value; }
        }

        /// <summary>
        /// Gets or sets the "from" field.
        /// </summary>
        /// <value>"From" field.</value>
        public string From
        {
            get { return from; }
            set { from = value; }
        }

        /// <summary>
        /// Gets or sets the "to" address list. The individual addresses have to be separated by the ';' character.
        /// </summary>
        /// <value>"To" address list.</value>
        public string To
        {
            get { return to; }
            set { to = value; }
        }

        /// <summary>
        /// Gets or sets the "cc" address list. The individual addresses have to be separated by the ';' character.
        /// </summary>
        /// <value>"Cc" address list.</value>
        public string CC
        {
            get { return cc; }
            set { cc = value; }
        }

        /// <summary>
        /// Gets or sets the "Bcc" address list. The individual addresses have to be separated by the ';' character.
        /// </summary>
        /// <value>"Bcc" address list.</value>
        public string Bcc
        {
            get { return bcc; }
            set { bcc = value; }
        }

        /// <summary>
        /// Gets or sets the subject of the mail.
        /// </summary>
        /// <value>The subject of the mail.</value>
        public string Subject
        {
            get { return subject; }
            set { subject = value; }
        }

        /// <summary>
        /// Gets or sets the body of the mail.
        /// </summary>
        /// <value>The body of the mail.</value>
        public string Body
        {
            get { return body; }
            set { body = value; }
        }

        /// <summary>
        /// Parses the address list in form of a string with individual addresses separated by the ';' character. The parsed
        /// addresses are then stored into the specified <see cref="MailAddressCollection"/>.
        /// </summary>
        /// <param name="collection">The mail address collection the parsed addresses should be stored into.</param>
        /// <param name="addresses">The addresses in form of a string.</param>
        public static void ParseAddresses (MailAddressCollection collection, string addresses)
        {
            if (collection == null)
                throw new ArgumentNullException ("collection");                
            
            if (addresses == null)
                throw new ArgumentNullException ("addresses");                
            
            string[] addressesArray = addresses.Split (';');

            foreach (string address in addressesArray)
                collection.Add (address);
        }

        /// <summary>
        /// Internal task execution code.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute (ITaskContext context)
        {
            using (SmtpClient smtpClient = new SmtpClient(smtpServer))
            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress(from);
                msg.Subject = subject;
                msg.Body = body;

                if (bcc != null)
                    ParseAddresses(msg.Bcc, bcc);
                if (cc != null)
                    ParseAddresses(msg.CC, cc);
                if (to != null)
                    ParseAddresses(msg.To, to);

                smtpClient.Send(msg);
            }
        }

        private string smtpServer;
        private string from;
        private string to;
        private string cc;
        private string bcc;
        private string subject;
        private string body;
    }
}
