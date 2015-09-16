// 
// IncidentTest.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalFire.Test
{
    [TestClass]
    public class IncidentTest
    {
        [TestMethod]
        public void CreateIncident_ValidTest()
        {
            Incident incident = new Incident(1, "Name", new Dictionary<string, string>());
            Assert.IsNotNull(incident);
            Assert.AreEqual(1, incident.ID);
            Assert.AreEqual("Name", incident.Name);
            Assert.AreEqual(0, incident.Details.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CreateIncident_InvalidIdTest()
        {
            Incident incident = new Incident(Incident.InvalidId, "Name", new Dictionary<string, string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateIncident_EmptyNameTest()
        {
            Incident incident = new Incident(1, "", new Dictionary<string, string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateIncident_NullNameTest()
        {
            Incident incident = new Incident(1, null, new Dictionary<string, string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateIncident_WhitespaceNameTest()
        {
            Incident incident = new Incident(1, " ", new Dictionary<string, string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateIncident_NullDetailsTest()
        {
            Incident incident = new Incident(1, "Name", null);
        }

        [TestMethod]
        public void CanRefresh_TrueTest()
        {
            Incident incident = new Incident(1, "Name", new Dictionary<string, string>());
            Assert.IsNotNull(incident);
            Assert.IsTrue(incident.CanRefresh());
        }

        [TestMethod]
        public void CanRefresh_FalseTest()
        {
            Incident incident = new Incident("Name", new Dictionary<string, string>());
            Assert.IsNotNull(incident);
            Assert.IsFalse(incident.CanRefresh());
        }

        [TestMethod]
        public void Refresh_ValidTest()
        {
            Incident incident = new Incident(100, "Name", new Dictionary<string, string>());
            Assert.IsNotNull(incident);
            Assert.AreEqual("Name", incident.Name);
            Assert.AreEqual(0, incident.Details.Count);

            incident.Refresh();
            Assert.AreEqual("29 Fire", incident.Name);
            Assert.AreEqual(20, incident.Details.Count);
        }

        [TestMethod]        
        public void Refresh_InvalidTest()
        {
            Incident incident = new Incident("Name", new Dictionary<string, string>());
            Assert.IsNotNull(incident);
            Assert.AreEqual("Name", incident.Name);
            Assert.AreEqual(0, incident.Details.Count);

            try
            {
                incident.Refresh();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(AggregateException));
                Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentOutOfRangeException));
            }
        }
    }
}
