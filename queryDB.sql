CREATE DATABASE CryptoAuthDB;
USE CryptoAuthDB;

CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(50) NOT NULL UNIQUE,
    given_names VARCHAR(50) NOT NULL,
    p_surname VARCHAR(50) NOT NULL, 
    m_surname VARCHAR(50), 
    phone_number VARCHAR(20) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    password_salt VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP,
    failed_attempts INT DEFAULT 0
);

DELIMITER $$
-- Procedimiento para insertar un nuevo usuario
CREATE PROCEDURE sp_insert_user(
    IN p_username VARCHAR(50),
    IN p_email VARCHAR(50),
    IN p_given_names VARCHAR(50),
    IN p_p_surname VARCHAR(50),
    IN p_m_surname VARCHAR(50),
    IN p_phone_number VARCHAR(20),
    IN p_password_hash VARCHAR(255),
    IN p_password_salt VARCHAR(255),
    OUT p_status_code INT,
    OUT p_message VARCHAR(255)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_status_code = 500;
        SET p_message = 'Error: No se pudo insertar el usuario.';
    END;

    START TRANSACTION;

    INSERT INTO users (username, email, given_names, p_surname, m_surname, phone_number, password_hash, password_salt)
    VALUES (p_username, p_email, p_given_names, p_p_surname, p_m_surname, p_phone_number, p_password_hash, p_password_salt);

    COMMIT;

    SET p_status_code = 200;
    SET p_message = 'Usuario insertado exitosamente.';
END$$

DELIMITER $$
-- Procedimiento para el inicio de sesión de un usuario
DELIMITER $$
-- Procedimiento para el inicio de sesión de un usuario
CREATE PROCEDURE sp_login_user(
    IN p_username VARCHAR(50),
    IN p_password_hash VARCHAR(255),
    OUT p_status_code INT,
    OUT p_message VARCHAR(255)
)
BEGIN
    DECLARE v_stored_hash VARCHAR(255);
    DECLARE v_failed_attempts INT;
    DECLARE v_max_attempts INT DEFAULT 5;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_status_code = 500;
        SET p_message = 'Error: No se pudo procesar el inicio de sesión.';
    END;

    START TRANSACTION;

    SELECT password_hash, failed_attempts
    INTO v_stored_hash, v_failed_attempts
    FROM users
    WHERE username = p_username;

    IF v_stored_hash IS NULL THEN
        ROLLBACK;
        SET p_status_code = 404;
        SET p_message = 'Error: Usuario no encontrado.';
    ELSEIF v_failed_attempts >= v_max_attempts THEN
        ROLLBACK;
        SET p_status_code = 423; -- Locked
        SET p_message = 'Error: Cuenta bloqueada por múltiples intentos fallidos. Contacte a soporte.';
    ELSEIF v_stored_hash = p_password_hash THEN
        UPDATE users
        SET last_login = CURRENT_TIMESTAMP, failed_attempts = 0
        WHERE username = p_username;

        COMMIT;
        SET p_status_code = 200;
        SET p_message = 'Inicio de sesión exitoso.';
    ELSE
        UPDATE users
        SET failed_attempts = failed_attempts + 1
        WHERE username = p_username;

        COMMIT;
        SET p_status_code = 401;
        SET p_message = CONCAT('Error: Contraseña incorrecta. Intentos restantes: ', (v_max_attempts - v_failed_attempts - 1));
    END IF;
END$$

DELIMITER $$
-- Procedimiento para obtener un usuario por email (Updated version)
CREATE PROCEDURE sp_get_user_by_email(
    IN p_email VARCHAR(50),
    OUT p_status_code INT,
    OUT p_message VARCHAR(255)
)
BEGIN
    DECLARE user_exists INT DEFAULT 0;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_status_code = 500;
        SET p_message = 'Error: No se pudo obtener el usuario.';
    END;

    START TRANSACTION;
    
    -- Check if user exists
    SELECT COUNT(*) INTO user_exists FROM users WHERE email = p_email;
    
    IF user_exists = 0 THEN
        SET p_status_code = 404;
        SET p_message = 'Error: Usuario no encontrado.';
    ELSE
        -- Return the user as a result set - Ensure failed_attempts is included
        SELECT 
            id, username, given_names, p_surname, m_surname, email, phone_number, 
            password_hash, password_salt, created_at, last_login, failed_attempts
        FROM users
        WHERE email = p_email;
        
        SET p_status_code = 200;
        SET p_message = 'Usuario obtenido exitosamente.';
    END IF;
    
    COMMIT;
END$$

DELIMITER $$
-- Procedimiento para obtener un usuario por username
CREATE PROCEDURE sp_get_user_by_username(
    IN p_username VARCHAR(50),
    OUT p_status_code INT,
    OUT p_message VARCHAR(255)
)
BEGIN
    DECLARE user_exists INT DEFAULT 0;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_status_code = 500;
        SET p_message = 'Error: No se pudo obtener el usuario.';
    END;

    START TRANSACTION;
    
    -- Check if user exists
    SELECT COUNT(*) INTO user_exists FROM users WHERE username = p_username;
    
    IF user_exists = 0 THEN
        SET p_status_code = 404;
        SET p_message = 'Error: Usuario no encontrado.';
    ELSE
        -- Return the user as a result set
        SELECT 
            id, username, given_names, p_surname, m_surname, email, phone_number, 
            password_hash, password_salt, created_at, last_login, failed_attempts
        FROM users
        WHERE username = p_username;
        
        SET p_status_code = 200;
        SET p_message = 'Usuario obtenido exitosamente.';
    END IF;
    
    COMMIT;
END$$
DELIMITER ;

DELIMITER $$
-- Procedimiento para verificar email
CREATE PROCEDURE sp_verify_email(
    IN p_user_id INT,
    OUT p_status_code INT,
    OUT p_message VARCHAR(255)
)
BEGIN
    DECLARE user_exists INT DEFAULT 0;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_status_code = 500;
        SET p_message = 'Error: No se pudo verificar el email.';
    END;

    START TRANSACTION;
    
    -- Check if user exists
    SELECT COUNT(*) INTO user_exists FROM users WHERE id = p_user_id;
    
    IF user_exists = 0 THEN
        SET p_status_code = 404;
        SET p_message = 'Error: Usuario no encontrado.';
    ELSE
        UPDATE users SET email_verified = TRUE WHERE id = p_user_id;
        
        SET p_status_code = 200;
        SET p_message = 'Email verificado exitosamente.';
    END IF;
    
    COMMIT;
END$$

DELIMITER $$
-- Procedimiento para cambiar contraseña
CREATE PROCEDURE sp_reset_password(
    IN p_user_id INT,
    IN p_new_password_hash VARCHAR(255),
    IN p_new_password_salt VARCHAR(255),
    OUT p_status_code INT,
    OUT p_message VARCHAR(255)
)
BEGIN
    DECLARE user_exists INT DEFAULT 0;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_status_code = 500;
        SET p_message = 'Error: No se pudo cambiar la contraseña.';
    END;

    START TRANSACTION;
    
    -- Check if user exists
    SELECT COUNT(*) INTO user_exists FROM users WHERE id = p_user_id;
    
    IF user_exists = 0 THEN
        SET p_status_code = 404;
        SET p_message = 'Error: Usuario no encontrado.';
    ELSE
        UPDATE users 
        SET password_hash = p_new_password_hash, 
            password_salt = p_new_password_salt,
            failed_attempts = 0
        WHERE id = p_user_id;
        
        SET p_status_code = 200;
        SET p_message = 'Contraseña cambiada exitosamente.';
    END IF;
    
    COMMIT;
END$$
DELIMITER ;