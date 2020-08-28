﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Protocols.TestManager.Kernel
{
    /// <summary>
    /// Test suite targets to .NET Framework.
    /// </summary>
    public class TestSuiteFromFramework : TestSuite
    {
        /// <summary>
        /// Loads the test suite assembly
        /// </summary>
        /// <param name="dllPath">A list of assemblies.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom")]
        public override void LoadFrom(IEnumerable<string> dllPath)
        {
            _testCaseList = new List<TestCase>();
            foreach (string DllFileName in dllPath)
            {
                Assembly assembly = Assembly.LoadFrom(DllFileName);
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    // Search for class, interfaces and other type
                    if (type.IsClass)
                    {
                        MethodInfo[] methods = type.GetMethods();
                        foreach (MethodInfo method in methods)
                        {
                            // Search for methods with TestMethodAttribute
                            object[] attributes = method.GetCustomAttributes(false);
                            bool isTestMethod = false;
                            bool isIgnored = false;
                            foreach (object attribute in attributes)
                            {
                                string name = attribute.GetType().Name;
                                // Break the loop when "IgnoreAttribute" is found
                                if (name == "IgnoreAttribute")
                                {
                                    isIgnored = true;
                                    break;
                                }

                                // Do not break the loop when "TestMethodAttribute" is found
                                // It's possible to have "IgnoreAttribute" after "TestMethodAttribute"
                                if (name == "TestMethodAttribute")
                                {
                                    isTestMethod = true;
                                }

                                // Ignore test case with TestCategory "Disabled"
                                if (name == "TestCategoryAttribute")
                                {
                                    PropertyInfo property = attribute.GetType().GetProperty("TestCategories");
                                    object category = property.GetValue(attribute, null);
                                    foreach (string str in (System.Collections.ObjectModel.ReadOnlyCollection<string>)category)
                                    {
                                        if (str == "Disabled")
                                        {
                                            isIgnored = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (isTestMethod && !isIgnored)
                            {
                                // Get categories and description
                                List<string> categories = new List<string>();
                                string description = null;
                                string caseFullName = method.DeclaringType.FullName + "." + method.Name;
                                foreach (object attribute in attributes)
                                {
                                    // Record TestCategories
                                    if (attribute.GetType().Name == "TestCategoryAttribute")
                                    {
                                        PropertyInfo property = attribute.GetType().GetProperty("TestCategories");
                                        object category = property.GetValue(attribute, null);
                                        foreach (string str in (System.Collections.ObjectModel.ReadOnlyCollection<string>)category)
                                        {
                                            categories.Add(str);
                                        }
                                    }

                                    // Record Description
                                    if (attribute.GetType().Name == "DescriptionAttribute")
                                    {
                                        var descriptionProp = attribute.GetType().GetProperty("Description");
                                        description = descriptionProp.GetValue(attribute, null) as string;
                                    }
                                }

                                TestCase testcase = new TestCase()
                                {
                                    FullName = caseFullName,
                                    Category = categories,
                                    Description = description,
                                    Name = method.Name
                                };

                                var testcaseToolTipBuilder = new StringBuilder();
                                testcaseToolTipBuilder.Append(testcase.Name);
                                if (testcase.Category.Any())
                                {
                                    testcaseToolTipBuilder.Append(Environment.NewLine + "Category:");
                                    foreach (var category in testcase.Category)
                                    {
                                        testcaseToolTipBuilder.Append(Environment.NewLine + "  " + category);
                                    }
                                }
                                if (!string.IsNullOrEmpty(testcase.Description))
                                {
                                    testcaseToolTipBuilder.Append(Environment.NewLine + "Description:");
                                    testcaseToolTipBuilder.Append(Environment.NewLine + "  " + testcase.Description);
                                }
                                testcase.ToolTipOnUI = testcaseToolTipBuilder.ToString();

                                _testCaseList.Add(testcase);
                            }
                        }
                    }
                }
            }
        }
    }
}
