// Assets/Editor/MacBuildFix.cs
#if UNITY_EDITOR && UNITY_EDITOR_OSX
using UnityEditor;
using UnityEditor.Callbacks;
using System.Diagnostics;
using System.IO;

public static class MacBuildFix
{
    // 빌드가 끝난 뒤 자동으로 호출
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.StandaloneOSX) return;

        // Create Xcode Project 체크했다면 .app이 아니라 .xcodeproj가 나옵니다 → 이 스크립트 스킵
        if (!pathToBuiltProject.EndsWith(".app")) return;

        var appPath = pathToBuiltProject;
        var macOSDir = Path.Combine(appPath, "Contents", "MacOS");
        var frameworksDir = Path.Combine(appPath, "Contents", "Frameworks");
        var pluginsDir = Path.Combine(appPath, "Contents", "Plugins");

        // 1) 격리(quarantine) 제거: 웹/클라우드 경유 시 실행 막힘 원인
        Run("/usr/bin/xattr", $"-dr com.apple.quarantine \"{appPath}\"");

        // 2) 실행 권한 부여: 실행 파일(+ 일부 러너블)에 +x 보장
        if (Directory.Exists(macOSDir))
            Run("/bin/chmod", $"+x \"{macOSDir}\"/*");

        // (선택) 혹시 하위 바이너리에 실행 비트 필요한 경우 대비해 재귀적으로 부여
        if (Directory.Exists(frameworksDir))
            Run("/bin/chmod", $"-R +x \"{frameworksDir}\"");
        if (Directory.Exists(pluginsDir))
            Run("/bin/chmod", $"-R +x \"{pluginsDir}\"");

        // 3) (선택) 로컬 실행 안정성↑: ad-hoc 서명(배포용 아님)
        //   - 다른 맥에 배포/다운로드할 땐 정식 서명+노타라이즈 필요
        Run("/usr/bin/codesign", $"--force --deep --sign - --timestamp=none \"{appPath}\"");

        // 4) (옵션) 빌드 끝나고 바로 실행하고 싶으면 주석 해제
        // Run("/usr/bin/open", $"\"{appPath}\"");

        UnityEngine.Debug.Log($"[MacBuildFix] Post process done for: {appPath}");
    }

    private static void Run(string fileName, string args)
    {
        var p = new Process();
        p.StartInfo.FileName = fileName;
        p.StartInfo.Arguments = args;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.Start();
        p.WaitForExit();
    }
}
#endif
