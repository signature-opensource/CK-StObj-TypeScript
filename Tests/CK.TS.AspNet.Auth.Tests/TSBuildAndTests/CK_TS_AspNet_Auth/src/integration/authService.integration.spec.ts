import axios from 'axios';
import { CookieJar } from 'tough-cookie';
import { wrapper as addCookieJar } from 'axios-cookiejar-support';

import {
    AuthService,
    IAuthenticationInfo,
    AuthLevel,
    IUserInfo
} from '../../ck-gen/src';
import { areUserInfoEquals, areAuthenticationInfoEquals } from '../helpers/test-helpers';

/*
 * These tests require a webfrontauth() in order to run them.
 * It needs to have:
 *  - Basic login enabled with one user matching the following pattern:
 *      {
 *          name: 'admin',
 *          password: 'admin'
 *      }
 *  - A not null sliding expiration
 */
describe('AuthService', function() {
    let authService: AuthService;

    const anonymous: IUserInfo = {
        userId: 0,
        userName: '',
        schemes: []
    };

    const logoutModel: IAuthenticationInfo = {
        user: anonymous,
        unsafeUser: anonymous,
        actualUser: anonymous,
        unsafeActualUser: anonymous,
        expires: undefined,
        criticalExpires: undefined,
        deviceId:"",
        isImpersonated: false,
        level: AuthLevel.None
    };

    beforeAll(async function() {
        const axiosInstance = axios.create();
        const cookieJar = new CookieJar();
        addCookieJar(axiosInstance);
        axiosInstance.defaults.jar = cookieJar;

        const identityEndPoint = {
            hostname: 'localhost',
            port: 27459,
            disableSsl: true
        };

        authService = await AuthService.createAsync( { identityEndPoint }, axiosInstance );
    });

    beforeEach(async function() {
        await authService.logout();
    });

    it('should basicLogin and logout.', async function() {
        await authService.basicLogin('admin', 'admin');
        let currentModel: IAuthenticationInfo = authService.authenticationInfo;
        expect(currentModel.user.userName).toBe('admin');
        expect(currentModel.unsafeUser.userName).toBe('admin');
        expect(currentModel.actualUser.userName).toBe('admin');
        expect(currentModel.unsafeActualUser.userName).toBe('admin');
        expect(currentModel.isImpersonated).toBe(false);
        expect(currentModel.level).toBe(AuthLevel.Normal);
        expect(authService.token).not.toBe('');
        expect(authService.refreshable).toBe(true);

        await authService.logout();
        currentModel = authService.authenticationInfo;
        expect(areUserInfoEquals(currentModel.user, anonymous)).toBe(true);
        expect(currentModel.unsafeUser.userName).toBe('admin');
        expect(areUserInfoEquals(currentModel.actualUser, anonymous)).toBe(true);
        expect(currentModel.unsafeActualUser.userName).toBe('admin');
        expect(currentModel.isImpersonated).toBe(false);
        expect(currentModel.level).toBe(AuthLevel.Unsafe);
        expect(authService.token).not.toBe('');
        expect(authService.refreshable).toBe(false);

        await authService.logout();
        expect(areAuthenticationInfoEquals(authService.authenticationInfo, logoutModel)).toBe(true);
        expect(authService.token).toBe('');
        expect(authService.refreshable).toBe(false);
    });

    it('should refresh correctly.', async function() {
        await authService.refresh();
        let currentModel: IAuthenticationInfo = authService.authenticationInfo;

        await authService.basicLogin('admin', 'admin');
        currentModel = authService.authenticationInfo;
        expect(currentModel.user.userName).toBe('admin');
        expect(currentModel.unsafeUser.userName).toBe('admin');
        expect(currentModel.actualUser.userName).toBe('admin');
        expect(currentModel.unsafeActualUser.userName).toBe('admin');
        expect(currentModel.isImpersonated).toBe(false);
        expect(currentModel.level).toBe(AuthLevel.Normal);
        expect(authService.token).not.toBe('');
        expect(authService.refreshable).toBe(true);

        await authService.refresh();
        currentModel = authService.authenticationInfo;
        expect(currentModel.user.userName).toBe('admin');
        expect(currentModel.unsafeUser.userName).toBe('admin');
        expect(currentModel.actualUser.userName).toEqual('admin');
        expect(currentModel.unsafeActualUser.userName).toBe('admin');
        expect(currentModel.isImpersonated).toBe(false);
        expect(currentModel.level).toBe(AuthLevel.Normal);
        expect(authService.token).not.toBe('');
        expect(authService.refreshable).toBe(true);

        await authService.logout();
        currentModel = authService.authenticationInfo;
        expect(areUserInfoEquals(currentModel.user, anonymous)).toBe(true);
        expect(currentModel.unsafeUser.userName).toBe('admin');
        expect(areUserInfoEquals(currentModel.actualUser, anonymous)).toBe(true);
        expect(currentModel.unsafeActualUser.userName).toBe('admin');
        expect(currentModel.isImpersonated).toBe(false);
        expect(currentModel.level).toBe(AuthLevel.Unsafe);
        expect(authService.token).not.toBe('');
        expect(authService.refreshable).toBe(false);

        await authService.logout();
        expect(areAuthenticationInfoEquals(authService.authenticationInfo, logoutModel)).toBe(true);
        expect(authService.token).toBe('');
        expect(authService.refreshable).toBe(false);
    });

    it('should call OnChange() correctly.', async function() {
        let authenticationInfo: IAuthenticationInfo = authService.authenticationInfo;
        const onChangeFunction = () => authenticationInfo = authService.authenticationInfo;
        authService.addOnChange(onChangeFunction);
        await authService.basicLogin('admin','admin');
        expect(areUserInfoEquals(authenticationInfo.user, anonymous)).toBe(false);
        await authService.logout();
        expect(areUserInfoEquals(authenticationInfo.user, anonymous)).toBe(true);

        authService.removeOnChange(onChangeFunction);
        await authService.basicLogin('admin','admin');
        expect(areUserInfoEquals(authenticationInfo.user, anonymous)).toBe(true);
    });
});
