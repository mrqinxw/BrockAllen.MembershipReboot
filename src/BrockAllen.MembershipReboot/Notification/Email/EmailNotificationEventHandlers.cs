﻿/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BrockAllen.MembershipReboot
{
    public class EmailEventHandler<TAccount>
        where TAccount: UserAccount
    {
        IMessageFormatter<TAccount> messageFormatter;
        IMessageDelivery messageDelivery;

        public EmailEventHandler(IMessageFormatter<TAccount> messageFormatter)
            : this(messageFormatter, new SmtpMessageDelivery())
        {
        }

        public EmailEventHandler(IMessageFormatter<TAccount> messageFormatter, IMessageDelivery messageDelivery)
        {
            if (messageFormatter == null) throw new ArgumentNullException("messageFormatter");
            if (messageDelivery == null) throw new ArgumentNullException("messageDelivery");

            this.messageFormatter = messageFormatter;
            this.messageDelivery = messageDelivery;
        }

        public virtual void Process(UserAccountEvent<TAccount> evt, object extra = null)
        {
            Tracing.Information("[{0}] Processing Event: {1}", this.GetType(), evt.GetType());
            
            var data = new Dictionary<string, string>();
            if (extra != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(extra))
                {
                    object obj2 = descriptor.GetValue(extra);
                    if (obj2 != null)
                    {
                        data.Add(descriptor.Name, obj2.ToString());
                    }
                }
            }

            var msg = this.messageFormatter.Format(evt, data);
            if (msg != null)
            {
                if (data.ContainsKey("NewEmail"))
                {
                    msg.To = data["NewEmail"];
                }
                else
                {
                    msg.To = evt.Account.Email;
                }

                msg = CustomizeEmail(msg, evt, data);
                
                if (msg != null && !String.IsNullOrWhiteSpace(msg.To))
                {
                    this.messageDelivery.Send(msg);
                }
            }
        }

        /// <summary>
        /// Apply transformations on the <paramref name="message"/> just before it is sent
        /// </summary>
        /// <remarks>
        /// Tip: Override this method to changes things like the email recipient(s)
        /// <para>
        /// A note to subclass implementors: whatever message is returned by this override will be the one that 
        /// is sent. The message you return can be the same as the <paramref name="message"/> supplied.
        /// If you do not return a message then no email will be sent.
        /// </para>
        /// </remarks>
        /// <param name="message"></param>
        /// <param name="evt"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual Message CustomizeEmail(Message message, UserAccountEvent<TAccount> evt, Dictionary<string, string> data)
        {
            return message;
        }
    }

    public class EmailAccountEventsHandler<T> :
        EmailEventHandler<T>,
        IEventHandler<AccountCreatedEvent<T>>,        
        IEventHandler<PasswordResetRequestedEvent<T>>,
        IEventHandler<PasswordChangedEvent<T>>,
        IEventHandler<PasswordResetSecretAddedEvent<T>>,
        IEventHandler<PasswordResetSecretRemovedEvent<T>>,
        IEventHandler<UsernameReminderRequestedEvent<T>>,
        IEventHandler<AccountApprovedEvent<T>>,
        IEventHandler<AccountRejectedEvent<T>>,
        IEventHandler<AccountClosedEvent<T>>,
        IEventHandler<AccountReopenedEvent<T>>,
        IEventHandler<AccountUnlockedEvent<T>>,
        IEventHandler<UsernameChangedEvent<T>>,
        IEventHandler<EmailChangeRequestedEvent<T>>,
        IEventHandler<EmailChangedEvent<T>>,
        IEventHandler<EmailVerifiedEvent<T>>,
        IEventHandler<MobilePhoneChangedEvent<T>>,
        IEventHandler<MobilePhoneRemovedEvent<T>>,
        IEventHandler<CertificateAddedEvent<T>>,
        IEventHandler<CertificateRemovedEvent<T>>,
        IEventHandler<LinkedAccountAddedEvent<T>>,
        IEventHandler<LinkedAccountRemovedEvent<T>>
        where T : UserAccount
    {
        public EmailAccountEventsHandler(IMessageFormatter<T> messageFormatter)
            : base(messageFormatter)
        {
        }
        public EmailAccountEventsHandler(IMessageFormatter<T> messageFormatter, IMessageDelivery messageDelivery)
            : base(messageFormatter, messageDelivery)
        {
        }

        public void Handle(AccountCreatedEvent<T> evt)
        {
            Process(evt, new { evt.InitialPassword, evt.VerificationKey });
        }
        
        public void Handle(PasswordResetRequestedEvent<T> evt)
        {
            Process(evt, new { evt.VerificationKey });
        }

        public void Handle(PasswordChangedEvent<T> evt)
        {
            Process(evt);
        }

        public void Handle(PasswordResetSecretAddedEvent<T> evt)
        {
            Process(evt);
        }

        public void Handle(PasswordResetSecretRemovedEvent<T> evt)
        {
            Process(evt);
        }
        
        public void Handle(UsernameReminderRequestedEvent<T> evt)
        {
            Process(evt);
        }

        public void Handle(AccountClosedEvent<T> evt)
        {
            Process(evt);
        }

        public void Handle(AccountApprovedEvent<T> evt)
        {
            Process(evt);
        }

        public void Handle(AccountRejectedEvent<T> evt)
        {
            Process(evt);
        }
        
        public void Handle(AccountReopenedEvent<T> evt)
        {
            Process(evt, new { evt.VerificationKey });
        }

        public void Handle(AccountUnlockedEvent<T> evt)
        {
            Process(evt);
        }

        public void Handle(UsernameChangedEvent<T> evt)
        {
            Process(evt);
        }

        public void Handle(EmailChangeRequestedEvent<T> evt)
        {
            Process(evt, new{evt.OldEmail, evt.NewEmail, evt.VerificationKey});
        }

        public void Handle(EmailChangedEvent<T> evt)
        {
            Process(evt, new { evt.OldEmail, evt.VerificationKey });
        }
        
        public void Handle(EmailVerifiedEvent<T> evt)
        {
            Process(evt);
        }

        public void Handle(MobilePhoneChangedEvent<T> evt)
        {
            Process(evt);
        }

        public void Handle(MobilePhoneRemovedEvent<T> evt)
        {
            Process(evt);
        }

        public void Handle(CertificateAddedEvent<T> evt)
        {
            Process(evt, new { evt.Certificate.Thumbprint, evt.Certificate.Subject });
        }

        public void Handle(CertificateRemovedEvent<T> evt)
        {
            Process(evt, new { evt.Certificate.Thumbprint, evt.Certificate.Subject });
        }

        public void Handle(LinkedAccountAddedEvent<T> evt)
        {
            Process(evt, new { evt.LinkedAccount.ProviderName });
        }

        public void Handle(LinkedAccountRemovedEvent<T> evt)
        {
            Process(evt, new { evt.LinkedAccount.ProviderName });
        }
    }

    public class EmailAccountEventsHandler : EmailAccountEventsHandler<UserAccount>
    {
        public EmailAccountEventsHandler(IMessageFormatter<UserAccount> messageFormatter)
            : base(messageFormatter)
        {
        }
        public EmailAccountEventsHandler(IMessageFormatter<UserAccount> messageFormatter, IMessageDelivery messageDelivery)
            : base(messageFormatter, messageDelivery)
        {
        }
    }
}
