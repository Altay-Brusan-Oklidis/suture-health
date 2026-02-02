module.exports = async (browser) => {
  const page = await browser.newPage();

  await page.setViewport({ width: 1024, height: 600 });

  await page.goto('https://app.ci.suturesign.com/request/sign', {
    waitUntil: 'load',
    timeout: 0,
  });
  await page.goto('https://app.ci.suturesign.com/request/sign');

  const usernameInput = await page.$('#Input_Username');

  if (!usernameInput) {
    await page.close();

    return;
  }

  await page.type('#Input_Username', process.env.USERNAME);
  await page.type('#Input_Password', process.env.PASSWORD);

  await page.click('button[type="submit"]');

  await page.waitForNavigation({ waitUntil: 'networkidle2' });
  await page.close();
};
