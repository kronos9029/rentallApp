import json
import re
import ssl
import time
import urllib.parse
import urllib.request
import http.cookiejar

BASE_URL = "https://localhost:7048"


def create_opener():
    ssl_context = ssl.create_default_context()
    ssl_context.check_hostname = False
    ssl_context.verify_mode = ssl.CERT_NONE
    cookie_jar = http.cookiejar.CookieJar()
    return urllib.request.build_opener(
        urllib.request.HTTPSHandler(context=ssl_context),
        urllib.request.HTTPCookieProcessor(cookie_jar),
    )


def get(opener, path_or_url):
    url = path_or_url if path_or_url.startswith("http") else BASE_URL + path_or_url
    with opener.open(url) as response:
        return response.read().decode("utf-8"), response.geturl(), response.status


def post(opener, path_or_url, data):
    url = path_or_url if path_or_url.startswith("http") else BASE_URL + path_or_url
    encoded = urllib.parse.urlencode(data).encode("utf-8")
    request = urllib.request.Request(url, data=encoded, method="POST")
    request.add_header("Content-Type", "application/x-www-form-urlencoded")
    with opener.open(request) as response:
        return response.read().decode("utf-8"), response.geturl(), response.status


def get_token(html):
    match = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', html)
    if not match:
        raise RuntimeError("Missing antiforgery token")
    return match.group(1)


def get_hidden_value(html, name):
    pattern = r'name="%s"[^>]*value="([^"]*)"' % re.escape(name)
    match = re.search(pattern, html)
    if not match:
        raise RuntimeError(f"Missing hidden input: {name}")
    return match.group(1)


def assert_contains(html, text, label):
    if text not in html:
        raise RuntimeError(f"{label} missing text: {text}")


def main():
    results = {}
    new_email = f"smoke.{int(time.time())}@local.test"
    initial_password = "Customer123!"
    reset_password = "Reset1234!"

    admin = create_opener()
    login_html, _, _ = get(admin, "/Auth/Login")
    login_token = get_token(login_html)
    post(
        admin,
        "/Auth/Login",
        {
            "__RequestVerificationToken": login_token,
            "Input.Email": "admin@local.test",
            "Input.Password": "Admin123!",
            "Input.RememberMe": "false",
        },
    )
    profile_html, _, _ = get(admin, "/Account/Profile")
    admin_html, _, _ = get(admin, "/Admin")
    assert_contains(profile_html, "admin@local.test", "admin profile")
    assert_contains(admin_html, "Admin Dashboard", "admin area")
    results["AdminLogin"] = "PASS"
    results["ProfileAuthorized"] = "PASS"
    results["AdminAuthorized"] = "PASS"

    register = create_opener()
    register_html, _, _ = get(register, "/Auth/Register")
    register_token = get_token(register_html)
    register_html, _, _ = post(
        register,
        "/Auth/Register",
        {
            "__RequestVerificationToken": register_token,
            "Input.FullName": "Smoke Customer",
            "Input.Email": new_email,
            "Input.Password": initial_password,
            "Input.ConfirmPassword": initial_password,
        },
    )
    assert_contains(register_html, "Tao tai khoan thanh cong", "register")
    results["RegisterCustomer"] = "PASS"

    forgot = create_opener()
    forgot_html, _, _ = get(forgot, "/Auth/ForgotPassword")
    forgot_token = get_token(forgot_html)
    forgot_html, _, _ = post(
        forgot,
        "/Auth/ForgotPassword",
        {
            "__RequestVerificationToken": forgot_token,
            "Input.Email": new_email,
        },
    )
    assert_contains(forgot_html, "Dev reset link", "forgot password")
    reset_link_match = re.search(r'href="(https://localhost:7048/Auth/ResetPassword[^"]+)"', forgot_html)
    if not reset_link_match:
        raise RuntimeError("Missing reset link")
    reset_url = reset_link_match.group(1).replace("&amp;", "&")
    reset_html, _, _ = get(forgot, reset_url)
    reset_form_token = get_token(reset_html)
    reset_email = get_hidden_value(reset_html, "Input.Email")
    reset_token = get_hidden_value(reset_html, "Input.Token")
    reset_html, _, _ = post(
        forgot,
        "/Auth/ResetPassword",
        {
            "__RequestVerificationToken": reset_form_token,
            "Input.Email": reset_email,
            "Input.Token": reset_token,
            "Input.Password": reset_password,
            "Input.ConfirmPassword": reset_password,
        },
    )
    assert_contains(reset_html, "Dat lai mat khau thanh cong", "reset password")
    results["ForgotPassword"] = "PASS"
    results["ResetPassword"] = "PASS"

    customer = create_opener()
    customer_login_html, _, _ = get(customer, "/Auth/Login")
    customer_login_token = get_token(customer_login_html)
    post(
        customer,
        "/Auth/Login",
        {
            "__RequestVerificationToken": customer_login_token,
            "Input.Email": new_email,
            "Input.Password": reset_password,
            "Input.RememberMe": "false",
        },
    )
    customer_profile_html, _, _ = get(customer, "/Account/Profile")
    assert_contains(customer_profile_html, "Smoke Customer", "customer profile")
    assert_contains(customer_profile_html, new_email, "customer profile")
    results["CustomerLoginAfterReset"] = "PASS"
    results["CustomerProfileAuthorized"] = "PASS"

    print(json.dumps(results, ensure_ascii=False))


if __name__ == "__main__":
    main()
