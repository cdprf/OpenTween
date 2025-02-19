﻿// OpenTween - Client of Twitter
// Copyright (c) 2015 kim_upsilon (@kim_upsilon) <https://upsilo.net/~upsilon/>
// All rights reserved.
//
// This file is part of OpenTween.
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option)
// any later version.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License
// for more details.
//
// You should have received a copy of the GNU General Public License along
// with this program. If not, see <http://www.gnu.org/licenses/>, or write to
// the Free Software Foundation, Inc., 51 Franklin Street - Fifth Floor,
// Boston, MA 02110-1301, USA.

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTween.Api.DataModel;
using OpenTween.SocialProtocol.Twitter;

namespace OpenTween
{
    public partial class SendErrorReportForm : OTBaseForm
    {
        public ErrorReport ErrorReport
        {
            get => this.errorReport;
            set
            {
                this.errorReport = value;
                this.bindingSource.DataSource = value;
            }
        }

        private ErrorReport errorReport = null!;

        public SendErrorReportForm()
            => this.InitializeComponent();

        private void SendErrorReportForm_Shown(object sender, EventArgs e)
        {
            this.pictureBoxIcon.Image = SystemIcons.Error.ToBitmap();
            this.textBoxErrorReport.DeselectAll();
        }

        private void ButtonReset_Click(object sender, EventArgs e)
            => this.ErrorReport.Reset();

        private async void ButtonSendByMail_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            await this.ErrorReport.SendByMailAsync();
        }

        private async void ButtonSendByDM_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;

            try
            {
                await this.ErrorReport.SendByDmAsync();

                MessageBox.Show(Properties.Resources.SendErrorReport_DmSendCompleted, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (WebApiException ex)
            {
                var message = Properties.Resources.SendErrorReport_DmSendError + Environment.NewLine + "Err:" + ex.Message;
                MessageBox.Show(message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonNotSend_Click(object sender, EventArgs e)
            => this.DialogResult = DialogResult.Cancel;
    }

    public class ErrorReport : NotifyPropertyChangedBase
    {
        public string ReportText
        {
            get => this.reportText;
            set
            {
                this.SetProperty(ref this.reportText, value);
                this.UpdateEncodedReport();
            }
        }

        private string reportText = "";

        public bool AnonymousReport
        {
            get => this.anonymousReport;
            set
            {
                this.SetProperty(ref this.anonymousReport, value);
                this.UpdateEncodedReport();
            }
        }

        private bool anonymousReport = true;

        public bool CanSendByDM
        {
            get => this.canSendByDm;
            private set => this.SetProperty(ref this.canSendByDm, value);
        }

        private bool canSendByDm;

        public string EncodedReportForDM
        {
            get => this.encodedReportForDM;
            private set => this.SetProperty(ref this.encodedReportForDM, value);
        }

        private string encodedReportForDM = "";

        private readonly TwitterLegacy? tw;
        private readonly string originalReportText;

        public ErrorReport(string reportText)
            : this(null, reportText)
        {
        }

        public ErrorReport(TwitterLegacy? tw, string reportText)
        {
            this.tw = tw;
            this.originalReportText = reportText;

            this.Reset();
        }

        public void Reset()
            => this.ReportText = this.originalReportText;

        public async Task SendByMailAsync()
        {
            var toAddress = ApplicationSettings.FeedbackEmailAddress;
            var subject = $"{ApplicationSettings.ApplicationName} {MyCommon.GetReadableVersion()} エラーログ";
            var body = this.ReportText;

            var mailto = $"mailto:{Uri.EscapeDataString(toAddress)}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
            await Task.Run(() => Process.Start(mailto));
        }

        public async Task SendByDmAsync()
        {
            if (!this.CheckDmAvailable())
                return;

            await this.tw!.SendDirectMessage(this.EncodedReportForDM);
        }

        private void UpdateEncodedReport()
        {
            if (!this.CheckDmAvailable())
            {
                this.CanSendByDM = false;
                return;
            }

            var body = $"Anonymous: {this.AnonymousReport}" + Environment.NewLine + this.ReportText;
            var originalBytes = Encoding.UTF8.GetBytes(body);

            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress, leaveOpen: true))
                {
                    gzipStream.Write(originalBytes, 0, originalBytes.Length);
                }

                var encodedReport = Convert.ToBase64String(outputStream.ToArray());
                var destScreenName = ApplicationSettings.FeedbackTwitterName.Substring(1);
                this.EncodedReportForDM = $"D {destScreenName} ErrorReport: {encodedReport}";
            }

            this.CanSendByDM = this.tw!.GetTextLengthRemain(this.EncodedReportForDM) >= 0;
        }

        private bool CheckDmAvailable()
        {
            if (!ApplicationSettings.AllowSendErrorReportByDM)
                return false;

            if (this.tw == null)
                return false;

            if (this.tw.AccountState.HasUnrecoverableError)
                return false;

            return true;
        }
    }
}
