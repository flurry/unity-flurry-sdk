/*
 * Copyright 2018, Oath Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using System.Diagnostics;
using System;

namespace FlurrySDK.Editor {
    public class PodInstalling {
        private const string FRAMEWORK_TARGET_PATH = "Pods/Flurry-iOS-SDK"; // relative to build output folder
     
        private const string FRAMEWORK_NAME_FLURRY = "Flurry_iOS_SDK";
        private const string FRAMEWORK_NAME_CONFIG = "Flurry_Config";

        // comment out if build non messaging package
        private const string FRAMEWORK_NAME_MESSAGING = "Flurry_Messaging";


        [PostProcessBuildAttribute(40)]
        public static void OnPostprocessBuild(BuildTarget target, string path) {

            if (target != BuildTarget.iOS){
                return;
            }

            // pod install to download xcframework dependency

            ShellCommand.AddPossibleRubySearchPaths();

            var currentDirectory = Directory.GetCurrentDirectory();

            // copy podfile
            var podFilePath = Path.Combine(path, "Podfile");
            var bundledPodfile = "Assets/Plugins/iOS/Editor/Podfile";

            var originalPodPath = Path.Combine(currentDirectory, bundledPodfile);
            File.Copy(originalPodPath, podFilePath);

            Directory.SetCurrentDirectory(path);

            // pod install
            ShellCommand.Run("pod", "install");
            Directory.SetCurrentDirectory(currentDirectory);

            // configure xcode project
            ConfigureXcodeForCocoaPods(path);
            

            // add xcframeworks to UnityFramework "Link binary with libraries"
            string pbxProjectPath = PBXProject.GetPBXProjectPath(path);
            PBXProject project = new PBXProject();
            project.ReadFromFile(pbxProjectPath);
     
            string targetGuid = project.TargetGuidByName("UnityFramework");
            
            CopyXCFramework(FRAMEWORK_NAME_FLURRY, project, targetGuid);
            CopyXCFramework(FRAMEWORK_NAME_CONFIG, project, targetGuid);

            // comment out if build non messaging package
            CopyXCFramework(FRAMEWORK_NAME_MESSAGING, project, targetGuid);
     
            project.WriteToFile(pbxProjectPath);

        }

        private static void CopyXCFramework(string n, PBXProject project, string targetGuid){
            string name = n + ".xcframework";
            string destPath = Path.Combine(FRAMEWORK_TARGET_PATH, name);

            // add the xcframework to copy bundled resources phase
            string fileGuid = project.AddFile(destPath, destPath);
            project.AddFileToBuild(targetGuid, fileGuid);
            
            // add the xcframework to link binary with libraries phase
            var unityLinkPhaseGuid = project.GetFrameworksBuildPhaseByTarget(targetGuid);
            project.AddFileToBuildSection(targetGuid, unityLinkPhaseGuid, fileGuid);


        }

        private static void CopyDirectory(string sourcePath, string destPath){
            Directory.CreateDirectory(destPath);
     
            foreach (string file in Directory.GetFiles(sourcePath))
                File.Copy(file, Path.Combine(destPath, Path.GetFileName(file)));
     
            foreach (string dir in Directory.GetDirectories(sourcePath))
                CopyDirectory(dir, Path.Combine(destPath, Path.GetFileName(dir)));
        }

        static void ConfigureXcodeForCocoaPods(string projectRoot) {
            var path = PBXProject.GetPBXProjectPath(projectRoot);
            var project = new PBXProject();
            project.ReadFromFile(path);
            var target = project.GetUnityFrameworkTargetGuid();

            project.SetBuildProperty(target, "GCC_PREPROCESSOR_DEFINITIONS", "$(inherited)");
            project.SetBuildProperty(target, "CLANG_ENABLE_MODULES", "YES");

            project.WriteToFile(path);
        }

    }
}