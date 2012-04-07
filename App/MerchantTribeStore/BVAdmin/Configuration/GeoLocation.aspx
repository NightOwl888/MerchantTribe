<%@ Page Title="" Language="C#" MasterPageFile="~/BVAdmin/BVAdminNav.master" AutoEventWireup="true" CodeBehind="GeoLocation.aspx.cs" Inherits="MerchantTribeStore.BVAdmin.Configuration.GeoLocation" %>
<%@ Register Src="../Controls/MessageBox.ascx" TagName="MessageBox" TagPrefix="uc1" %>
<%@ Register src="NavMenu.ascx" tagname="NavMenu" tagprefix="uc2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headcontent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="NavContent" runat="server">
    <uc2:NavMenu ID="NavMenu1" runat="server" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <uc1:MessageBox ID="MessageBox1" runat="server" />
    <h1>Geo-Location</h1>
    <table class="formtable">
    <tr>
        <td class="formlabel">Time Zone:</td>
        <td class="formfield"><asp:DropDownList ID="lstTimeZone" runat="server"></asp:DropDownList></td>
    </tr>
    <tr>
        <td class="formlabel">Culture/Currency Settings:</td>
        <td class="formfield"><asp:DropDownList ID="lstCulture" runat="server"></asp:DropDownList></td>
    </tr>
    <tr>
        <td class="formlabel">Midpoint Rounding Preference:</td>
        <td class="formfield"><asp:DropDownList ID="lstMidpointRoundingMode" runat="server"></asp:DropDownList></td>
    </tr>
    <tr>
        <td>&nbsp;</td>
        <td class="formfield"><asp:ImageButton ID="btnSubmit" runat="server" 
                ImageUrl="~/bvadmin/images/buttons/savechanges.png" onclick="btnSubmit_Click" /></td>
    </tr>
    </table>
</asp:Content>
