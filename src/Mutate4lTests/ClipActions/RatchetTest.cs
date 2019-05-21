﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mutate4lTests.ClipActions
{
    [TestClass]
    public class RatchetTest
    {
        [TestMethod]
        public void TestRatchetPitchWithByClip()
        {
            byte[] input = {97, 0, 0, 2, 0, 0, 0, 0, 128, 64, 1, 6, 0, 65, 0, 0, 160, 63, 0, 0, 0, 63, 100, 65, 0, 0, 0, 64, 0, 0, 128, 63, 100, 67, 0, 0, 64, 64, 0, 0, 0, 63, 100, 68, 0, 0, 0, 0, 0, 0, 64, 63, 100, 69, 0, 0, 128, 63, 0, 0, 0, 63, 100, 72, 0, 0, 96, 64, 0, 0, 0, 63, 100, 0, 1, 0, 0, 128, 64, 1, 5, 0, 60, 0, 0, 0, 0, 0, 0, 64, 63, 100, 62, 0, 0, 128, 63, 0, 0, 64, 63, 100, 62, 0, 0, 64, 64, 0, 0, 0, 63, 100, 64, 0, 0, 0, 64, 0, 0, 128, 63, 100, 72, 0, 0, 96, 64, 0, 0, 0, 63, 100, 91, 48, 93, 32, 114, 97, 116, 99, 104, 101, 116, 32, 45, 98, 121, 32, 91, 49, 93};
            byte[] output = {97, 0, 0, 0, 128, 64, 1, 28, 0, 68, 0, 0, 0, 0, 0, 0, 64, 63, 100, 69, 0, 0, 128, 63, 170, 170, 42, 62, 100, 69, 85, 85, 149, 63, 173, 170, 42, 62, 100, 65, 0, 0, 160, 63, 170, 170, 42, 62, 100, 69, 171, 170, 170, 63, 170, 170, 42, 62, 100, 65, 85, 85, 181, 63, 173, 170, 42, 62, 100, 65, 171, 170, 202, 63, 170, 170, 42, 62, 100, 65, 0, 0, 0, 64, 205, 204, 76, 62, 100, 65, 205, 204, 12, 64, 205, 204, 76, 62, 100, 65, 154, 153, 25, 64, 205, 204, 76, 62, 100, 65, 102, 102, 38, 64, 205, 204, 76, 62, 100, 65, 51, 51, 51, 64, 205, 204, 76, 62, 100, 67, 0, 0, 64, 64, 170, 170, 42, 62, 100, 67, 171, 170, 74, 64, 173, 170, 42, 62, 100, 67, 85, 85, 85, 64, 170, 170, 42, 62, 100, 72, 0, 0, 96, 64, 217, 137, 29, 61, 100, 72, 39, 118, 98, 64, 222, 137, 29, 61, 100, 72, 79, 236, 100, 64, 206, 137, 29, 61, 100, 72, 118, 98, 103, 64, 220, 137, 29, 61, 100, 72, 158, 216, 105, 64, 220, 137, 29, 61, 100, 72, 197, 78, 108, 64, 220, 137, 29, 61, 100, 72, 236, 196, 110, 64, 220, 137, 29, 61, 100, 72, 20, 59, 113, 64, 206, 137, 29, 61 , 100, 72, 59, 177, 115, 64, 220, 137, 29, 61, 100, 72, 98, 39, 118, 64, 220, 137, 29, 61, 100, 72, 138, 157, 120, 64, 220, 137, 29, 61, 100, 72, 177, 19, 123, 64, 206, 137, 29, 61, 100, 72, 217, 137, 125, 64, 220, 137, 29, 61, 100};
            
            TestUtilities.InputShouldProduceGivenOutput(input, output);
        }
        
        // Test generated by mutate4l from formula: [0] ratchet 1 2 3 5
        [TestMethod]
        public void TestRatchetDirectValues()
        {
            byte[] input = { 98, 0, 0, 1, 0, 0, 0, 0, 128, 64, 1, 6, 0, 65, 0, 0, 160, 63, 0, 0, 0, 63, 100, 65, 0, 0, 0, 64, 0, 0, 128, 63, 100, 67, 0, 0, 64, 64, 0, 0, 0, 63, 100, 68, 0, 0, 0, 0, 0, 0, 64, 63, 100, 69, 0, 0, 128, 63, 0, 0, 0, 63, 100, 72, 0, 0, 96, 64, 0, 0, 0, 63, 100, 91, 48, 93, 32, 114, 97, 116, 99, 104, 101, 116, 32, 49, 32, 50, 32, 51, 32, 53 };
            byte[] output = { 98, 0, 0, 0, 128, 64, 1, 14, 0, 68, 0, 0, 0, 0, 0, 0, 64, 63, 100, 69, 0, 0, 128, 63, 0, 0, 128, 62, 100, 65, 0, 0, 160, 63, 170, 170, 42, 62, 100, 69, 0, 0, 160, 63, 0, 0, 128, 62, 100, 65, 85, 85, 181, 63, 173, 170, 42, 62, 100, 65, 171, 170, 202, 63, 170, 170, 42, 62, 100, 65, 0, 0, 0, 64, 205, 204, 76, 62, 100, 65, 205, 204, 12, 64, 205, 204, 76, 62, 100, 65, 154, 153, 25, 64, 205, 204, 76, 62, 100, 65, 102, 102, 38, 64, 205, 204, 76, 62, 100, 65, 51, 51, 51, 64, 205, 204, 76, 62, 100, 67, 0, 0, 64, 64, 0, 0, 0, 63, 100, 72, 0, 0, 96, 64, 0, 0, 128, 62, 100, 72, 0, 0, 112, 64, 0, 0, 128, 62, 100 };

            TestUtilities.InputShouldProduceGivenOutput(input, output);
        }
    }
}