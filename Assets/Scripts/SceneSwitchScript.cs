using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class SceneSwitchScript : MonoBehaviour {

    public Material skyMat;
    public GameObject NetworkManagerGameObject;

    public void switchToHost() {
        SceneManager.LoadScene("Host Scene");
    }

    public void switchToParticipant() {
        SceneManager.LoadScene("Main Scene");
    }

    public void switchToKitchenARKit() {
        SceneManager.LoadScene("Kitchen ARKit");
    }

    public void switchToKitchenGLEAM() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Kitchen GLEAM");
    }

    public void switchToKitchenGLEAM1() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Kitchen GLEAM 1");
    }

    public void switchToKitchenGLEAM2() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Kitchen GLEAM 2");
    }

    public void switchToKitchenGLEAM3() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Kitchen GLEAM 3");
    }

    public void switchToKitchenGLEAM4() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Kitchen GLEAM 4");
    }

    public void switchToKitchenGLEAM5() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Kitchen GLEAM 5");
    }

    public void switchToKitchenNo() {
        SceneManager.LoadScene("Kitchen No");
    }

    public void switchToKitchenDirect() {
        SceneManager.LoadScene("Kitchen Directional");
    }

    public void switchToKitchenMain() {

        Debug.Log("Resetting tecture");
        skyMat.SetTexture("_Tex", null);

        SceneManager.LoadScene("Kitchen Main");

    }

    public void switchToChessARKit() {
        SceneManager.LoadScene("Chess ARKit");
    }

    public void switchToChessGLEAM() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Chess GLEAM");
    }

    public void switchToChessGLEAM1() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Chess GLEAM 1");
    }

    public void switchToChessGLEAM2() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Chess GLEAM 2");
    }

    public void switchToChessGLEAM3() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Chess GLEAM 3");
    }

    public void switchToChessGLEAM4() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Chess GLEAM 4");
    }

    public void switchToChessGLEAM5() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Chess GLEAM 5");
    }

    public void switchToChessNo() {
        SceneManager.LoadScene("Chess No");
    }

    public void switchToChessDirect() {
        SceneManager.LoadScene("Chess Directional");
    }

    public void switchToChessMain() {
        
        Debug.Log("Resetting tecture");
        skyMat.SetTexture("_Tex", null);
        SceneManager.LoadScene("Chess Main");
    }

    public void switchToWoodARKit() {
        SceneManager.LoadScene("Wood ARKit");
    }

    public void switchToWoodGLEAM() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Wood GLEAM");
        
    }

    public void switchToWoodGLEAM1() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Wood GLEAM 1");

    }

    public void switchToWoodGLEAM2() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Wood GLEAM 2");

    }

    public void switchToWoodGLEAM3() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Wood GLEAM 3");

    }

    public void switchToWoodGLEAM4() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Wood GLEAM 4");

    }

    public void switchToWoodGLEAM5() {
        Destroy(NetworkManagerGameObject);
        NetworkManager.Shutdown();

        SceneManager.LoadScene("Wood GLEAM 5");

    }

    public void switchToWoodNo() {
        SceneManager.LoadScene("Wood No");
    }

    public void switchToWoodDirect() {
        SceneManager.LoadScene("Wood Directional");
    }

    public void switchToWoodMain() {
        
        Debug.Log("Resetting tecture");
        skyMat.SetTexture("_Tex", null);
        SceneManager.LoadScene("Wood Main");
    }

    public void switchToMain() {
        SceneManager.LoadScene("Main Scene");
    }

    public void switchToLanding() {
        SceneManager.LoadScene("Landing Scene");
    }

    public void switchToMatMain() {

        //Debug.Log("Resetting tecture");
        skyMat.SetTexture("_Tex", null);
        SceneManager.LoadScene("Material Main");
    }


    public void switchToGLEAMMat() {
        SceneManager.LoadScene("GLEAM Scene Materials");
    }

    public void switchToARKITMat1() {
        SceneManager.LoadScene("ARKit2 Image Target Scene 1");
    }

    public void switchToARKITMat2() {
        SceneManager.LoadScene("ARKit2 Image Target Scene 2");
    }
    public void switchToARKITMat3() {
        SceneManager.LoadScene("ARKit2 Image Target Scene 3");
    }
    public void switchToARKITMat4() {
        SceneManager.LoadScene("ARKit2 Image Target Scene 4");
    }
    public void switchToARKITMat5() {
        SceneManager.LoadScene("ARKit2 Image Target Scene 5");
    }
    public void switchToARKITMat6() {
        SceneManager.LoadScene("ARKit2 Image Target Scene 6");
    }
    public void switchToARKITMat7() {
        SceneManager.LoadScene("ARKit2 Image Target Scene");
    }
}
