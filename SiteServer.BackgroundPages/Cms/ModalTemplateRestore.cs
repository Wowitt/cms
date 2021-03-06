﻿using System;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using SiteServer.BackgroundPages.Core;
using SiteServer.Utils;
using SiteServer.CMS.Database.Core;
using SiteServer.CMS.Fx;

namespace SiteServer.BackgroundPages.Cms
{
    public class ModalTemplateRestore : BasePageCms
    {
        public PlaceHolder PhContent;
        public DropDownList DdlLogId;
        public TextBox TbContent;

        private int _templateId;
        private string _includeUrl;
        private int _logId;

        protected override bool IsSinglePage => true;

	    public static string GetOpenWindowString(int siteId, int templateId, string includeUrl)
        {
            return LayerUtils.GetOpenScript("还原历史版本", FxUtils.GetCmsUrl(siteId, nameof(ModalTemplateRestore), new NameValueCollection
            {
                {"templateID", templateId.ToString()},
                {"includeUrl", includeUrl}
            }));
        }

		public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            FxUtils.CheckRequestParameter("siteId");

            _templateId = AuthRequest.GetQueryInt("templateID");
            _includeUrl = AuthRequest.GetQueryString("includeUrl");
            _logId = AuthRequest.GetQueryInt("logID");

            if (IsPostBack) return;

            var logDictionary = DataProvider.TemplateLog.GetLogIdWithNameDictionary(_templateId);
            if (logDictionary.Count > 0)
            {
                PhContent.Visible = true;
                foreach (var value in logDictionary)
                {
                    var listItem = new ListItem(value.Value, value.Key.ToString());
                    DdlLogId.Items.Add(listItem);
                }
                if (_logId > 0)
                {
                    ControlUtils.SelectSingleItem(DdlLogId, _logId.ToString());
                }

                if (_logId == 0)
                {
                    _logId = TranslateUtils.ToInt(DdlLogId.Items[0].Value);
                }
                TbContent.Text = DataProvider.TemplateLog.GetTemplateContent(_logId);
            }
            else
            {
                PhContent.Visible = false;
                InfoMessage("当前模板不存在历史版本，无法进行还原");
            }
        }

        public void DdlLogId_SelectedIndexChanged(object sender, EventArgs e)
        {
            FxUtils.Redirect(FxUtils.GetCmsUrl(SiteId, nameof(ModalTemplateRestore), new NameValueCollection
            {
                {"templateID", _templateId.ToString()},
                {"includeUrl", _includeUrl},
                {"logID", DdlLogId.SelectedValue}
            }));
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            var templateLogId = TranslateUtils.ToInt(DdlLogId.SelectedValue);
            if (templateLogId == 0)
            {
                FailMessage("当前模板不存在历史版本，无法进行还原");
            }
            else
            {
                LayerUtils.CloseAndRedirect(Page, PageTemplateAdd.GetRedirectUrlToRestore(SiteId, _templateId, templateLogId));
            }
        }
	}
}
