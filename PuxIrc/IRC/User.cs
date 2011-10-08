using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuxIrc.IRC
{
	/*!
	 * \brief	represents some aggregated data about a user
	 * */
	public class User
	{
		public User()
		{
			m_nick = "puxirc_guest";
		}

		public String Nick
		{
			get { return m_nick; }
			set { m_nick = value; }
		}

		private String m_nick;
	}
}
