using Mtf.Network.Services;
using NUnit.Framework;
using System;

namespace Mtf.Network.UnitTest.Services
{
    [TestFixture]
    public class BufferHelperTests
    {
        [Test]
        public void GetNext_Int_ReturnsExpectedValue()
        {
            byte[] buffer = { 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 };
            var start = 0;

            var value = BufferHelper.GetNextInt(buffer, ref start);

            Assert.Multiple(() =>
            {
                Assert.That(value, Is.EqualTo(1));
                Assert.That(start, Is.EqualTo(4));
            });
        }

        [Test]
        public void GetNext_Long_ReturnsExpectedValue()
        {
            byte[] buffer = { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            var start = 0;

            var value = BufferHelper.GetNextLong(buffer, ref start);

            Assert.Multiple(() =>
            {
                Assert.That(value, Is.EqualTo(1L));
                Assert.That(start, Is.EqualTo(8));
            });
        }

        [Test]
        public void GetNext_ThrowsException_WhenBufferIsNull()
        {
            byte[] buffer = null;
            var start = 0;

            Assert.Throws<ArgumentNullException>(() => BufferHelper.GetNextInt(buffer, ref start));
        }

        [Test]
        public void GetNext_ThrowsException_WhenStartOutOfRange()
        {
            byte[] buffer = { 0x01, 0x00, 0x00, 0x00 };
            var start = 5;

            Assert.Throws<ArgumentOutOfRangeException>(() => BufferHelper.GetNextInt(buffer, ref start));
        }

        [Test]
        public void GetNext_ThrowsException_WhenBufferTooSmallForType()
        {
            byte[] buffer = { 0x01, 0x00 };
            var start = 0;

            Assert.Throws<ArgumentException>(() => BufferHelper.GetNextInt(buffer, ref start));
        }
    }
}