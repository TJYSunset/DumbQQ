using System.Text.RegularExpressions;

namespace DumbQQ.Constants
{
    internal static class Api
    {
        public static readonly (string url, string referer) GetQrCode = (
            "https://ssl.ptlogin2.qq.com/ptqrshow?appid=501004106&e=0&l=M&s=5&d=72&v=4&t=0.1",
            ""
            );

        public static readonly (string url, string referer) VerifyQrCode = (
            "https://ssl.ptlogin2.qq.com/ptqrlogin?ptqrtoken={0}&webqq_type=10&remember_uin=1&login2qq=1&aid=501004106&u1=https%3A%2F%2Fw.qq.com%2Fproxy.html%3Flogin2qq%3D1%26webqq_type%3D10&ptredirect=0&ptlang=2052&daid=164&from_ui=1&pttype=1&dumy=&fp=loginerroralert&0-0-157510&mibao_css=m_webqq&t=undefined&g=1&js_type=0&js_ver=10184&login_sig=&pt_randsalt=3"
            ,
            "https://ui.ptlogin2.qq.com/cgi-bin/login?daid=164&target=self&style=16&mibao_css=m_webqq&appid=501004106&enable_qlogin=0&no_verifyimg=1&s_url=https%3A%2F%2Fw.qq.com%2Fproxy.html&f_url=loginerroralert&strong_login=1&login_state=10&t=20131024001"
            );

        public static readonly (string url, string referer) GetPtwebqq = (
            "{0}",
            null
            );

        public static readonly (string url, string referer) GetVfwebqq = (
            "https://s.web2.qq.com/api/getvfwebqq?ptwebqq={0}&clientid=53999199&psessionid=&t=0.1",
            "https://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1"
            );

        public static readonly (string url, string referer) GetUinAndPsessionid = (
            "https://d1.web2.qq.com/channel/login2",
            "https://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2"
            );

        public static readonly (string url, string referer) GetGroupList = (
            "https://s.web2.qq.com/api/get_group_name_list_mask2",
            "https://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2"
            );

        public static readonly (string url, string referer) PollMessage = (
            "https://d1.web2.qq.com/channel/poll2",
            "https://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2"
            );

        public static readonly (string url, string referer) SendMessageToGroup = (
            "https://d1.web2.qq.com/channel/send_qun_msg2",
            "https://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2"
            );

        public static readonly (string url, string referer) GetFriendList = (
            "https://s.web2.qq.com/api/get_user_friends2",
            "https://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1"
            );

        public static readonly (string url, string referer) SendMessageToFriend = (
            "https://d1.web2.qq.com/channel/send_buddy_msg2",
            "https://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2"
            );

        public static readonly (string url, string referer) GetDiscussList = (
            "https://s.web2.qq.com/api/get_discus_list?clientid=53999199&psessionid={0}&vfwebqq={1}&t=0.1",
            "https://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1"
            );

        public static readonly (string url, string referer) SendMessageToDiscuss = (
            "https://d1.web2.qq.com/channel/send_discu_msg2",
            "https://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2"
            );

        public static readonly (string url, string referer) GetAccountInfo = (
            "https://s.web2.qq.com/api/get_self_info2?t=0.1",
            "https://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1"
            );

        public static readonly (string url, string referer) GetRecentList = (
            "https://d1.web2.qq.com/channel/get_recent_list2",
            "https://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2"
            );

        public static readonly (string url, string referer) GetFriendStatus = (
            "https://d1.web2.qq.com/channel/get_online_buddies2?vfwebqq={0}&clientid=53999199&psessionid={1}&t=0.1",
            "https://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2"
            );

        public static readonly (string url, string referer) GetGroupInfo = (
            "https://s.web2.qq.com/api/get_group_info_ext2?gcode={0}&vfwebqq={1}&t=0.1",
            "https://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1"
            );

        public static readonly (string url, string referer) GetQQById = (
            "https://s.web2.qq.com/api/get_friend_uin2?tuin={0}&type=1&vfwebqq={1}&t=0.1",
            "https://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1"
            );

        public static readonly (string url, string referer) GetDiscussInfo = (
            "https://d1.web2.qq.com/channel/get_discu_info?did={0}&vfwebqq={1}&clientid=53999199&psessionid={2}&t=0.1",
            "https://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2"
            );

        public static readonly (string url, string referer) GetFriendInfo = (
            "https://s.web2.qq.com/api/get_friend_info2?tuin={0}&vfwebqq={1}&clientid=53999199&psessionid={2}&t=0.1",
            "https://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1"
            );

        public static readonly Regex GetPtwebqqPattern = new Regex(@"https[^']+?(?=')");
    }
}