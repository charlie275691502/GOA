/* (C)2019 Rayark Inc. - All Rights Reserved
 * Rayark Confidential
 *
 * NOTICE: The intellectual and technical concepts contained herein are
 * proprietary to or under control of Rayark Inc. and its affiliates.
 * The information herein may be covered by patents, patents in process,
 * and are protected by trade secret or copyright law.
 * You may not disseminate this information or reproduce this material
 * unless otherwise prior agreed by Rayark Inc. in writing.
 */

using NUnit.Framework;
using NSubstitute;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity;

namespace GamePlayHub
{
    public class GamePlayHubFunc {


        [Test] public void A() { CalculatePower(new List<int>(3) { 0, 1, 3 }, 1, 6); }
        [Test] public void B() { CalculatePower(new List<int>(3) { 0, 1, 13 }, 1, 3); }
        [Test] public void C() { CalculatePower(new List<int>(10) { 0, 3, 4, 33, 36, 40, 49, 53, 64, 65 }, 1, 25); }
        [Test] public void D() { CalculatePower(new List<int>(15) { 6, 13, 31, 32, 34, 37, 42, 47, 50, 55, 56, 57, 61, 77, 79 }, 1, 30); }
        [Test] public void E() { CalculatePower(new List<int>(12) { 1, 2, 16, 18, 20, 22, 23, 24, 35, 48, 51, 52 }, 1, 31); }
        [Test] public void F() { CalculatePower(new List<int>(17) { 3, 6, 9, 10, 13, 14, 24, 27, 28, 32, 33, 37, 45, 49, 51, 65, 81 }, 2, 56); }

        void CalculatePower(List<int> nameCodes, int maskMultiplier, int expected) {
            int power = Func.CalculatePower(nameCodes, maskMultiplier, new List<PowerType>(4){ PowerType.Wealth, PowerType.Industry, PowerType.SeaPower, PowerType.Military});
            
            Assert.True(power == expected);
        }
    }
}