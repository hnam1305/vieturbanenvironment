using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System.Net.Mail;
using TMPro;
using UnityEngine;

public class FirebaseController : MonoBehaviour
{
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject signUpPanel;
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private GameObject resetPasswordPanel;
    [SerializeField] private GameObject loginError;
    [SerializeField] private GameObject signUpError;

    [SerializeField] private TMP_InputField loginEmail;
    [SerializeField] private TMP_InputField loginPassword;
    [SerializeField] private TMP_InputField signUpEmail;
    [SerializeField] private TMP_InputField signUpUsername;
    [SerializeField] private TMP_InputField signUpPassword;
    [SerializeField] private TMP_InputField signUpCPassword;
    [SerializeField] private TMP_InputField resetPEmail;
    [SerializeField] private TMP_Text userName;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TMP_Text email;
    [SerializeField] private TMP_Text signUpErrorText;
    [SerializeField] private TMP_Text loginErrorText;

    private FirebaseAuth auth;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
    }
    private void ShowPanel(GameObject target)
    {
        loginPanel.SetActive(loginPanel == target);
        signUpPanel.SetActive(signUpPanel == target);
        profilePanel.SetActive(profilePanel == target);
        resetPasswordPanel.SetActive(resetPasswordPanel == target);
    }
    public void OpenLoginPage()
    {
        ShowPanel(loginPanel);
    }
    public void OpenSignUpPage()
    {
        ShowPanel(signUpPanel);
    }
    public void OpenProfilePage()
    {
        ShowPanel(profilePanel);
    }
    public void OpenResetPasswordPage()
    {
        ShowPanel(resetPasswordPanel);
    }
    public void Login()
    {
        if (auth == null) return;
        if (string.IsNullOrEmpty(loginEmail.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            ShowError("Vui lòng nhập email và mật khẩu.");
            return;
        }
        SignInUser(loginEmail.text.Trim(), loginPassword.text);
    }
    public void SignUp()
    {
        if (auth == null) return;

        string mail = signUpEmail.text.Trim();
        string name = signUpUsername.text.Trim();

        if (string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(name)
            || string.IsNullOrEmpty(signUpPassword.text) || string.IsNullOrEmpty(signUpCPassword.text))
        {
            ShowError("Vui lòng điền đủ thông tin.");
            return;
        }
        if (!IsValidEmail(mail))
        {
            ShowError("Email không hợp lệ.");
            return;
        }
        if (signUpPassword.text.Length < 6)
        {
            ShowError("Mật khẩu cần ít nhất 6 ký tự.");
            return;
        }
        if (signUpPassword.text != signUpCPassword.text)
        {
            ShowError("Mật khẩu xác nhận không khớp.");
            return;
        }
        CreateUser(mail, signUpPassword.text, name);
    }
    public void ForgetPassword()
    {
        if (string.IsNullOrEmpty(resetPEmail.text))
        {
            return;
        }
    }
    public void LogOut()
    {
        if (auth != null) auth.SignOut();
        userName.text = "";
        email.text = "";
        password.text = "";
        ClearAuthFields();
        OpenLoginPage();
    }
    private void CreateUser(string mail, string pass, string name)
    {
        auth.CreateUserWithEmailAndPasswordAsync(mail, pass).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Sign up failed: " + task.Exception);
                ShowError("Đăng ký thất bại. Email có thể đã được dùng.");
                return;
            }
            UpdateUserProfile(task.Result.User, name);
        });
    }
    private void UpdateUserProfile(FirebaseUser newUser, string name)
    {
        UserProfile profile = new UserProfile { DisplayName = name };
        newUser.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task =>
        {
            bool ok = !(task.IsCanceled || task.IsFaulted);
            if (!ok) Debug.LogError("Update profile failed: " + task.Exception);

            // Tài khoản đã được tạo dù đặt tên có lỗi hay không
            ShowProfile(ok ? name : newUser.DisplayName, newUser.Email);
        });
    }
    private void SignInUser(string mail, string pass)
    {
        auth.SignInWithEmailAndPasswordAsync(mail, pass).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Sign in failed: " + task.Exception);
                ShowError("Sai email hoặc mật khẩu.");
                return;
            }
            FirebaseUser signedIn = task.Result.User;
            ShowProfile(signedIn.DisplayName, signedIn.Email);
        });
    }
    private void ShowProfile(string name, string mail)
    {
        userName.text = name ?? "";
        email.text = mail ?? "";
        ClearAuthFields();
        OpenProfilePage();
    }
    private void ClearAuthFields()
    {
        loginEmail.text = "";
        loginPassword.text = "";
        signUpEmail.text = "";
        signUpUsername.text = "";
        signUpPassword.text = "";
        signUpCPassword.text = "";
    }
    private bool IsValidEmail(string value)
    {
        try
        {
            return new MailAddress(value).Address == value;
        }
        catch
        {
            return false;
        }
    }
    private void ShowError(string message)
    {
        Debug.LogWarning(message);
    }
}
